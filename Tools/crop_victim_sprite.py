"""
Extract Victim sheet row 1, column 4, then:
- strip black grid lines and grey checkerboard background to transparency
- crop to tight bounds around non-transparent pixels

Re-run after changing Victim.jpg:  py Tools/crop_victim_sprite.py
"""
from pathlib import Path

try:
    from PIL import Image
except ImportError:
    raise SystemExit("pip install Pillow")

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / "Assets" / "Sprites" / "NPC's" / "Victim.jpg"
# Output standalone sprite for Assets/Sprites/NPC's/Victim_Kitchen.png (VictimBody_Kitchen prefab).
OUT = ROOT / "Assets" / "Sprites" / "NPC's" / "Victim_Kitchen.png"

ROWS, COLS = 2, 6
# Inset inside the cell to drop outer grid strokes (pixels)
GRID_TRIM = 5


def is_background_pixel(r: int, g: int, b: int) -> bool:
    """Checkerboard tiles + JPEG noise: low chroma mid/high greys. Grid lines: very dark."""
    mx, mn = max(r, g, b), min(r, g, b)
    chroma = mx - mn
    lum = (r + g + b) / 3.0

    # Outer grid / separators
    if lum < 42:
        return True

    # Typical light/dark checker greys (sprite sheet empty cells)
    if chroma <= 38 and 68 < lum < 248:
        return True

    return False


def mask_checkerboard_rgba(im: Image.Image) -> Image.Image:
    px = im.load()
    w, h = im.size
    out = Image.new("RGBA", (w, h))
    po = out.load()
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if is_background_pixel(r, g, b):
                po[x, y] = (0, 0, 0, 0)
            else:
                po[x, y] = (r, g, b, 255)
    return out


def bbox_nontransparent(im: Image.Image, pad: int = 2):
    px = im.load()
    w, h = im.size
    min_x, min_y = w, h
    max_x, max_y = -1, -1
    for y in range(h):
        for x in range(w):
            if px[x, y][3] > 8:
                min_x = min(min_x, x)
                min_y = min(min_y, y)
                max_x = max(max_x, x)
                max_y = max(max_y, y)
    if max_x < 0:
        return 0, 0, w, h
    min_x = max(0, min_x - pad)
    min_y = max(0, min_y - pad)
    max_x = min(w - 1, max_x + pad)
    max_y = min(h - 1, max_y + pad)
    return min_x, min_y, max_x + 1, max_y + 1


def main():
    if not SRC.is_file():
        raise SystemExit(f"Missing source: {SRC}")

    im = Image.open(SRC).convert("RGBA")
    print("Source sheet size:", im.size)
    w, h = im.size
    cw, ch = w // COLS, h // ROWS
    r, c = 0, 3  # row1 col4
    x0, y0 = c * cw, r * ch
    x1, y1 = x0 + cw, y0 + ch
    cell = im.crop(
        (x0 + GRID_TRIM, y0 + GRID_TRIM, x1 - GRID_TRIM, y1 - GRID_TRIM)
    )
    print("Cell (trimmed) size:", cell.size)

    cleaned = mask_checkerboard_rgba(cell)
    bx0, by0, bx1, by1 = bbox_nontransparent(cleaned, pad=2)
    final = cleaned.crop((bx0, by0, bx1, by1))
    final.save(OUT, optimize=True)
    print("Saved:", OUT)
    print("Final sprite size:", final.size, "(tight crop around body)")


if __name__ == "__main__":
    main()
