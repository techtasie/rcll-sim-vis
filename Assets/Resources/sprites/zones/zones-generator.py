from PIL import Image, ImageDraw, ImageFont

# Constants
WIDTH, HEIGHT = 300, 300
ZONE_SIZE = 300  # Width and height of each zone
COLORS = {"M": "lightpink", "C": "lightcyan"}
FONT_PATH = (
    "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"  # Adjust path to your font
)
BORDER_WIDTH = 5  # Width of the zone border


# Generate an image with a single zone
def generate_single_zone_image(zone_type, row, col):
    # Create a new image with white background
    img = Image.new("RGB", (WIDTH, HEIGHT), "white")
    draw = ImageDraw.Draw(img)

    # Load the font
    try:
        font = ImageFont.truetype(FONT_PATH, 25)
    except:
        print("Font not found, using default font.")
        font = ImageFont.load_default()

    zone_color = COLORS[zone_type]
    zone_id = f"{zone_type}_Z{row + 1}{col + 1}"

    # Draw the zone rectangle with a bigger border
    draw.rectangle(
        [
            BORDER_WIDTH,
            BORDER_WIDTH,
            ZONE_SIZE - BORDER_WIDTH,
            ZONE_SIZE - BORDER_WIDTH,
        ],
        fill=zone_color,
        outline="black",
        width=BORDER_WIDTH,
    )

    # Draw the text in the top of the zone
    text_x, text_y = 10, 10
    draw.text((text_x, text_y), zone_id, fill="black", font=font)

    return img


if __name__ == "__main__":
    # Generate and save multiple images, each containing a single zone
    for row in range(7):
        for col in range(8):
            for zone_type in ["M", "C"]:
                zone_image = generate_single_zone_image(zone_type, row, col)
                zone_image.save(f"{zone_type}_Z{row + 1}{col + 1}.png")

                # Optionally, display the image
                # zone_image.show()
