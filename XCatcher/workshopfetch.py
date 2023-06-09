import os
import concurrent.futures
import requests
from bs4 import BeautifulSoup

STEAM_WORKSHOP_PATH = r'C:\Program Files (x86)\Steam\steamapps\workshop\content\255710'
MOD_LIST_PATH = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'XCMODLIST.txt')

def retrieve_workshop_item_names():
    workshop_folder_ids = get_workshop_folder_ids(STEAM_WORKSHOP_PATH)
    if workshop_folder_ids:
        workshop_item_names = search_workshop_item_names(workshop_folder_ids)
        save_workshop_item_names(workshop_item_names)
    else:
        print("No workshop folder IDs found.")

def get_workshop_folder_ids(path):
    folder_ids = []
    if os.path.exists(path):
        for folder_name in os.listdir(path):
            folder_path = os.path.join(path, folder_name)
            if os.path.isdir(folder_path):
                folder_ids.append(folder_name)
    return folder_ids

def search_workshop_item_name(folder_id):
    url = f"https://steamcommunity.com/sharedfiles/filedetails/?id={folder_id}"
    response = requests.get(url)
    if response.status_code == 200:
        soup = BeautifulSoup(response.content, 'html.parser')
        item_name_element = soup.find('div', class_='workshopItemTitle')
        if item_name_element:
            return folder_id, item_name_element.get_text(strip=True)
    return None

def search_workshop_item_names(folder_ids):
    workshop_item_names = {}
    with concurrent.futures.ThreadPoolExecutor() as executor:
        future_to_folder_id = {executor.submit(search_workshop_item_name, folder_id): folder_id for folder_id in folder_ids}
        for future in concurrent.futures.as_completed(future_to_folder_id):
            folder_id = future_to_folder_id[future]
            result = future.result()
            if result:
                workshop_item_names[result[0]] = result[1]
    return workshop_item_names

def save_workshop_item_names(workshop_item_names):
    num_folders = len(workshop_item_names)
    if num_folders > 0:
        with open(MOD_LIST_PATH, 'w', encoding='utf-8') as file:
            for folder_id, item_name in workshop_item_names.items():
                file.write(f"{folder_id}: {item_name}\n")
        print(f"{num_folders} mods were found.")
    else:
        print("No workshop item names found.")

# Call the function to retrieve the mod list
retrieve_workshop_item_names()
