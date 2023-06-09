import os
import shutil
import datetime

# Function to extract data from .crp files
def extract_data_from_crp_files():
    def extract_crp(crp_file_path, output_file_path):
        with open(crp_file_path, 'rb') as crp_file:
            with open(output_file_path, 'wb') as output_file:
                shutil.copyfileobj(crp_file, output_file)

        print(f"Extraction complete for {crp_file_path}.")

    def convert_to_text(binary_file_path, output_file_path, encoding='utf-8', errors='replace'):
        with open(binary_file_path, 'rb') as binary_file:
            binary_data = binary_file.read()
            try:
                text = binary_data.decode(encoding, errors=errors)
            except UnicodeDecodeError:
                text = binary_data.decode(encoding, errors='ignore')

            with open(output_file_path, 'w', encoding=encoding) as output_file:
                output_file.write(text)

        print(f"Conversion complete for {binary_file_path}.")

    # Function to extract city name and mods from the end of the extracted binary file
    def extract_city_and_mods(text):
        lines = text.split('\n')
        city_name = lines[-2]
        mods = lines[-1].split(': ')[-1].split(', ')
        return city_name, mods

    # Function to create the XCLOG file
    def create_report(crp_file_path, output_directory, mods):
        city_name = os.path.splitext(os.path.basename(crp_file_path))[0]
        report_file_name = "XCLOG.txt"
        report_file_path = os.path.join(output_directory, "XCATCHER", report_file_name)

        with open(report_file_path, 'a', encoding='utf-8') as report_file:
            report_file.write(f"{city_name}:\n")
            report_file.write(f"City: {city_name}\n")
            report_file.write(f"Mods: {', '.join(mods)}\n\n")

        print(f"XCATCHER REPORT updated for {city_name}.")

    # Get the current directory
    current_dir = os.path.join(os.getenv('LOCALAPPDATA'), 'Colossal Order', 'Cities_Skylines', 'Saves')

    # Create the XCATCHER folder if it doesn't exist
    xcatcher_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), "XCATCHER")
    os.makedirs(xcatcher_dir, exist_ok=True)

    # Search for .crp files in the current directory
    crp_files = [file for file in os.listdir(current_dir) if file.endswith(".crp")]

    # Iterate over the .crp files and extract their data
    for crp_file in crp_files:
        crp_file_path = os.path.join(current_dir, crp_file)
        output_file_name = f"{os.path.splitext(crp_file)[0]}_extracted.txt"
        output_file_path = os.path.join(xcatcher_dir, output_file_name)
        extract_crp(crp_file_path, output_file_path)

        # Convert the extracted binary .txt file to readable text
        convert_to_text(output_file_path, output_file_path)

        # Read the extracted text file and extract city name and mods
        with open(output_file_path, 'r', encoding='utf-8') as extracted_file:
            extracted_text = extracted_file.read()
            city_name, mods = extract_city_and_mods(extracted_text)

        # Create or update the XCATCHER REPORT file
        create_report(crp_file_path, os.path.dirname(os.path.abspath(__file__)), mods)

# Get the current date and time
current_date = datetime.datetime.now().date()
current_time = datetime.datetime.now().time()

# Create the XCLOG header
xclog_header = f"XCATCHER REPORT LOG\n{current_date}\n{current_time}\n\nRECOMMENDATION: SEND THIS LOG FILE TO NYOKO'S SERVER FOR SUPPORT: https://discord.gg/GqGbeUD8Dg\n"

# Call the function to extract data from .crp files
extract_data_from_crp_files()

# Write the XCLOG header to the XCLOG.txt file
report_file_name = "XCLOG.txt"
report_file_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "XCATCHER", report_file_name)

with open(report_file_path, 'r+', encoding='utf-8') as report_file:
    content = report_file.read()
    report_file.seek(0, 0)
    report_file.write(xclog_header + content)
