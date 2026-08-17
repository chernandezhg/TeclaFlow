from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parent
ASSETS = ROOT / "Assets"
ASSETS.mkdir(exist_ok=True)


def font(size: int, bold: bool = False) -> ImageFont.FreeTypeFont:
    name = "segoeuib.ttf" if bold else "segoeui.ttf"
    return ImageFont.truetype(str(Path("C:/Windows/Fonts") / name), size)


def rounded_key(draw: ImageDraw.ImageDraw, box: tuple[int, int, int, int], radius: int) -> None:
    x1, y1, x2, y2 = box
    draw.rounded_rectangle((x1, y1 + 5, x2, y2 + 5), radius=radius, fill="#302B86")
    draw.rounded_rectangle(box, radius=radius, fill="#625AF6", outline="#817BFF", width=2)


scale = 4
side = Image.new("RGB", (164 * scale, 314 * scale), "#171D31")
d = ImageDraw.Draw(side)

rounded_key(d, (34 * scale, 34 * scale, 130 * scale, 126 * scale), 25 * scale)
t_font = font(58 * scale, bold=True)
t_box = d.textbbox((0, 0), "T", font=t_font)
t_width = t_box[2] - t_box[0]
d.text(((82 * scale - t_width / 2), 39 * scale), "T", font=t_font, fill="white")

title_font = font(20 * scale, bold=True)
subtitle_font = font(10 * scale)
d.text((27 * scale, 154 * scale), "TeclaFlow", font=title_font, fill="white")
d.text((31 * scale, 182 * scale), "ESCRITURA ASISTIDA", font=subtitle_font, fill="#AEB4C6")

for index, width in enumerate((74, 58, 86)):
    y = (231 + index * 17) * scale
    d.rounded_rectangle((39 * scale, y, (39 + width) * scale, y + 4 * scale), radius=2 * scale, fill="#635BFF")
d.rectangle((31 * scale, 224 * scale, 34 * scale, 276 * scale), fill="#58E2FF")

side = side.resize((164, 314), Image.Resampling.LANCZOS)
side.save(ASSETS / "wizard-side.bmp")

small = Image.new("RGB", (55 * scale, 55 * scale), "white")
sd = ImageDraw.Draw(small)
rounded_key(sd, (6 * scale, 5 * scale, 49 * scale, 47 * scale), 12 * scale)
small_t_font = font(27 * scale, bold=True)
small_t_box = sd.textbbox((0, 0), "T", font=small_t_font)
small_t_width = small_t_box[2] - small_t_box[0]
sd.text((27.5 * scale - small_t_width / 2, 7 * scale), "T", font=small_t_font, fill="white")
small = small.resize((55, 55), Image.Resampling.LANCZOS)
small.save(ASSETS / "wizard-small.bmp")

print(f"Generated {ASSETS / 'wizard-side.bmp'}")
print(f"Generated {ASSETS / 'wizard-small.bmp'}")
