import customtkinter as ctk
import json
import os
import subprocess
import shutil
import math
from tkinter import filedialog
from PIL import Image, ImageTk

ctk.set_appearance_mode("dark")
ctk.set_default_color_theme("blue")

CONFIG_FILE = "config.json"
MODS_FOLDER = "mods"
MODS_META_FILE = "mods_meta.json"

def load_config():
    if os.path.exists(CONFIG_FILE):
        with open(CONFIG_FILE, "r") as f:
            return json.load(f)
    return {"game_path": "", "bg_image": ""}

def save_config(config):
    with open(CONFIG_FILE, "w") as f:
        json.dump(config, f)

def load_mods_meta():
    if os.path.exists(MODS_META_FILE):
        with open(MODS_META_FILE, "r") as f:
            return json.load(f)
    return {}

def save_mods_meta(meta):
    with open(MODS_META_FILE, "w") as f:
        json.dump(meta, f)

if not os.path.exists(MODS_FOLDER):
    os.makedirs(MODS_FOLDER)


class SplashScreen(ctk.CTkToplevel):
    def __init__(self, parent):
        super().__init__(parent)
        self.overrideredirect(True)
        w, h = 480, 280
        sw = self.winfo_screenwidth()
        sh = self.winfo_screenheight()
        self.geometry(f"{w}x{h}+{(sw-w)//2}+{(sh-h)//2}")
        self.configure(fg_color="#0d0d0d")
        self.lift()
        self.attributes("-topmost", True)
        ctk.CTkLabel(self, text="StoryForge", font=ctk.CTkFont(size=36, weight="bold"), text_color="white").pack(pady=(50, 6))
        ctk.CTkLabel(self, text="MCSM Mod Manager", font=ctk.CTkFont(size=14), text_color="gray").pack()
        ctk.CTkLabel(self, text="v0.4", font=ctk.CTkFont(size=12), text_color="#444").pack(pady=4)
        self.progress = ctk.CTkProgressBar(self, width=320)
        self.progress.pack(pady=30)
        self.progress.set(0)
        self.status = ctk.CTkLabel(self, text="Loading...", font=ctk.CTkFont(size=11), text_color="gray")
        self.status.pack()
        self.animate_progress()

    def animate_progress(self):
        steps = [
            (0.2, "Initializing..."),
            (0.5, "Loading config..."),
            (0.8, "Loading mods..."),
            (1.0, "Ready!"),
        ]
        def step(i):
            if i < len(steps):
                val, msg = steps[i]
                self.progress.set(val)
                self.status.configure(text=msg)
                self.after(400, lambda: step(i + 1))
        step(0)


class StoryForgeApp(ctk.CTk):
    def __init__(self):
        super().__init__()
        self.withdraw()
        self.config_data = load_config()
        self.mods_meta = load_mods_meta()
        self.title("StoryForge Launcher")
        self.geometry("900x600")
        self.resizable(False, False)
        if os.path.exists("icon.ico"):
            self.iconbitmap("icon.ico")
        self.bg_image_ref = None
        self.gradient_angle = 0
        self.after(200, self.show_splash)
        self.mainloop()

    def show_splash(self):
        splash = SplashScreen(self)
        self.after(2200, lambda: self.launch_main(splash))

    def launch_main(self, splash):
        try:
            splash.destroy()
        except Exception:
            pass
        self.build_layout()
        self.show_page("welcome")
        self.deiconify()
        self.animate_gradient()

    def animate_gradient(self):
        self.gradient_angle = (self.gradient_angle + 1) % 360
        r1 = int(20 + 15 * math.sin(math.radians(self.gradient_angle)))
        g1 = int(20 + 10 * math.sin(math.radians(self.gradient_angle + 120)))
        b1 = int(40 + 20 * math.sin(math.radians(self.gradient_angle + 240)))
        color = f"#{r1:02x}{g1:02x}{b1:02x}"
        try:
            self.sidebar.configure(fg_color=color)
        except Exception:
            pass
        self.after(50, self.animate_gradient)

    def build_layout(self):
        self.sidebar = ctk.CTkFrame(self, width=180, corner_radius=0)
        self.sidebar.pack(side="left", fill="y")
        self.sidebar.pack_propagate(False)
        ctk.CTkLabel(self.sidebar, text="StoryForge", font=ctk.CTkFont(size=20, weight="bold")).pack(pady=(20, 4))
        ctk.CTkLabel(self.sidebar, text="v0.4", font=ctk.CTkFont(size=11), text_color="gray").pack(pady=(0, 20))
        self.nav_buttons = {}
        nav_items = [
            ("welcome", "Home"),
            ("launcher", "Launcher"),
            ("mods", "Mod Manager"),
            ("settings", "Settings"),
            ("about", "About"),
        ]
        for key, label in nav_items:
            btn = ctk.CTkButton(
                self.sidebar, text=label, anchor="w", width=160,
                fg_color="transparent", hover_color="#2a2d2e",
                font=ctk.CTkFont(size=13),
                command=lambda k=key: self.show_page(k)
            )
            btn.pack(pady=4, padx=10)
            self.nav_buttons[key] = btn
        self.content = ctk.CTkFrame(self, corner_radius=0, fg_color="transparent")
        self.content.pack(side="left", fill="both", expand=True)

    def clear_content(self):
        for widget in self.content.winfo_children():
            widget.destroy()

    def show_page(self, page):
        self.current_page = page
        self.clear_content()
        self.draw_bg()
        for key, btn in self.nav_buttons.items():
            btn.configure(fg_color="#1f538d" if key == page else "transparent")
        pages = {
            "welcome": self.page_welcome,
            "launcher": self.page_launcher,
            "mods": self.page_mods,
            "settings": self.page_settings,
            "about": self.page_about,
        }
        pages[page]()

    def draw_bg(self):
        bg_path = self.config_data.get("bg_image", "")
        if bg_path and os.path.exists(bg_path):
            try:
                import tkinter as tk
                img = Image.open(bg_path).resize((720, 600))
                self.bg_image_ref = ImageTk.PhotoImage(img)
                bg_label = tk.Label(self.content, image=self.bg_image_ref)
                bg_label.place(x=0, y=0, relwidth=1, relheight=1)
            except Exception:
                pass

    def page_welcome(self):
        ctk.CTkLabel(self.content, text="Welcome to StoryForge!", font=ctk.CTkFont(size=28, weight="bold")).pack(pady=(40, 4))
        ctk.CTkLabel(self.content, text="Your Minecraft: Story Mode Mod Manager & launcher", font=ctk.CTkFont(size=14), text_color="gray").pack()
        news_frame = ctk.CTkFrame(self.content, corner_radius=12)
        news_frame.pack(pady=30, padx=40, fill="x")
        ctk.CTkLabel(news_frame, text="Latest News", font=ctk.CTkFont(size=16, weight="bold")).pack(pady=(16, 8), padx=20, anchor="w")
        news = [
            "StoryForge v0.4 is live!",
            "Splash screen added on startup",
            "Animated sidebar gradient added",
            "Background image support added in Settings",
            "Mod descriptions and version info added",
        ]
        for item in news:
            ctk.CTkLabel(news_frame, text=f"  {item}", font=ctk.CTkFont(size=13)).pack(pady=2, padx=30, anchor="w")
        ctk.CTkLabel(news_frame, text="").pack(pady=6)
        ctk.CTkButton(self.content, text="Get Started", width=200, height=45,
                      font=ctk.CTkFont(size=15, weight="bold"),
                      command=lambda: self.show_page("launcher")).pack(pady=20)

    def page_launcher(self):
        ctk.CTkLabel(self.content, text="Launcher", font=ctk.CTkFont(size=26, weight="bold")).pack(pady=(30, 4))
        ctk.CTkLabel(self.content, text="Launch Minecraft: Story Mode", font=ctk.CTkFont(size=13), text_color="gray").pack()
        ctk.CTkLabel(self.content, text="Game Executable:", font=ctk.CTkFont(size=13)).pack(pady=(30, 4))
        self.path_entry = ctk.CTkEntry(self.content, width=480, placeholder_text="Browse to your MCSM game.exe")
        self.path_entry.pack()
        if self.config_data["game_path"]:
            self.path_entry.insert(0, self.config_data["game_path"])
        ctk.CTkButton(self.content, text="Browse", width=120, command=self.browse_game).pack(pady=8)
        ctk.CTkButton(self.content, text="Launch Game", width=220, height=50,
                      font=ctk.CTkFont(size=16, weight="bold"),
                      command=self.launch_game).pack(pady=20)
        self.launch_status = ctk.CTkLabel(self.content, text="", font=ctk.CTkFont(size=12))
        self.launch_status.pack()

    def browse_game(self):
        path = filedialog.askopenfilename(filetypes=[("Executable", "*.exe")])
        if path:
            self.path_entry.delete(0, "end")
            self.path_entry.insert(0, path)
            self.config_data["game_path"] = path
            save_config(self.config_data)

    def launch_game(self):
        path = self.path_entry.get()
        if os.path.exists(path):
            subprocess.Popen([path])
            self.launch_status.configure(text="Game launched!", text_color="lightgreen")
        else:
            self.launch_status.configure(text="Game path not found.", text_color="red")

    def page_mods(self):
        ctk.CTkLabel(self.content, text="Mod Manager", font=ctk.CTkFont(size=26, weight="bold")).pack(pady=(30, 4))
        ctk.CTkLabel(self.content, text="Install and manage your MCSM mods", font=ctk.CTkFont(size=13), text_color="gray").pack()
        ctk.CTkButton(self.content, text="Install Mod", width=160, command=self.install_mod).pack(pady=(20, 8))
        self.mod_list_frame = ctk.CTkScrollableFrame(self.content, width=560, height=300, label_text="Installed Mods")
        self.mod_list_frame.pack(pady=8, padx=20)
        self.refresh_mod_list()

    def install_mod(self):
        path = filedialog.askopenfilename(filetypes=[("Mod files", "*.zip *.landb *.pak"), ("All files", "*.*")])
        if path:
            filename = os.path.basename(path)
            shutil.copy2(path, os.path.join(MODS_FOLDER, filename))
            if filename not in self.mods_meta:
                self.mods_meta[filename] = {"description": "No description", "version": "1.0"}
                save_mods_meta(self.mods_meta)
            self.refresh_mod_list()

    def refresh_mod_list(self):
        for widget in self.mod_list_frame.winfo_children():
            widget.destroy()
        mods = os.listdir(MODS_FOLDER)
        if not mods:
            ctk.CTkLabel(self.mod_list_frame, text="No mods installed yet.", text_color="gray").pack(pady=20)
            return
        for mod in mods:
            meta = self.mods_meta.get(mod, {"description": "No description", "version": "1.0"})
            card = ctk.CTkFrame(self.mod_list_frame, corner_radius=8)
            card.pack(fill="x", pady=4, padx=4)
            top_row = ctk.CTkFrame(card, fg_color="transparent")
            top_row.pack(fill="x", padx=8, pady=(8, 2))
            ctk.CTkLabel(top_row, text=mod, font=ctk.CTkFont(size=13, weight="bold"), anchor="w").pack(side="left")
            ctk.CTkLabel(top_row, text=f"v{meta['version']}", font=ctk.CTkFont(size=11), text_color="gray").pack(side="left", padx=8)
            ctk.CTkButton(top_row, text="Remove", width=80, height=26, fg_color="#8b0000",
                          hover_color="#a00000",
                          command=lambda m=mod: self.remove_mod(m)).pack(side="right", padx=4)
            ctk.CTkLabel(card, text=meta["description"], font=ctk.CTkFont(size=11),
                         text_color="gray", anchor="w").pack(fill="x", padx=12, pady=(0, 8))

    def remove_mod(self, mod_name):
        path = os.path.join(MODS_FOLDER, mod_name)
        if os.path.exists(path):
            os.remove(path)
            self.mods_meta.pop(mod_name, None)
            save_mods_meta(self.mods_meta)
            self.refresh_mod_list()

    def page_settings(self):
        ctk.CTkLabel(self.content, text="Settings", font=ctk.CTkFont(size=26, weight="bold")).pack(pady=(30, 4))
        ctk.CTkLabel(self.content, text="Configure StoryForge", font=ctk.CTkFont(size=13), text_color="gray").pack()
        ctk.CTkLabel(self.content, text="Appearance Mode:", font=ctk.CTkFont(size=13)).pack(pady=(20, 4))
        self.appearance_menu = ctk.CTkOptionMenu(self.content, values=["Dark", "Light", "System"],
                                                  command=self.change_appearance)
        self.appearance_menu.set(ctk.get_appearance_mode())
        self.appearance_menu.pack()
        ctk.CTkLabel(self.content, text="Game Path:", font=ctk.CTkFont(size=13)).pack(pady=(16, 4))
        self.settings_path = ctk.CTkEntry(self.content, width=480, placeholder_text="Path to MCSM game.exe")
        self.settings_path.pack()
        if self.config_data["game_path"]:
            self.settings_path.insert(0, self.config_data["game_path"])
        ctk.CTkLabel(self.content, text="Background Image:", font=ctk.CTkFont(size=13)).pack(pady=(16, 4))
        bg_row = ctk.CTkFrame(self.content, fg_color="transparent")
        bg_row.pack()
        self.bg_entry = ctk.CTkEntry(bg_row, width=380, placeholder_text="Path to background image (.png/.jpg)")
        self.bg_entry.pack(side="left", padx=(0, 8))
        if self.config_data.get("bg_image"):
            self.bg_entry.insert(0, self.config_data["bg_image"])
        ctk.CTkButton(bg_row, text="Browse", width=90, command=self.browse_bg).pack(side="left")
        ctk.CTkButton(self.content, text="Save Settings", width=160, command=self.save_settings).pack(pady=20)
        self.settings_status = ctk.CTkLabel(self.content, text="", font=ctk.CTkFont(size=12))
        self.settings_status.pack()

    def browse_bg(self):
        path = filedialog.askopenfilename(filetypes=[("Images", "*.png *.jpg *.jpeg *.webp")])
        if path:
            self.bg_entry.delete(0, "end")
            self.bg_entry.insert(0, path)

    def change_appearance(self, mode):
        ctk.set_appearance_mode(mode)

    def save_settings(self):
        self.config_data["game_path"] = self.settings_path.get()
        self.config_data["bg_image"] = self.bg_entry.get()
        save_config(self.config_data)
        self.settings_status.configure(text="Settings saved!", text_color="lightgreen")

    def page_about(self):
        ctk.CTkLabel(self.content, text="StoryForge", font=ctk.CTkFont(size=32, weight="bold")).pack(pady=(50, 4))
        ctk.CTkLabel(self.content, text="Version 0.4", font=ctk.CTkFont(size=14), text_color="gray").pack()
        ctk.CTkLabel(self.content, text="A Minecraft: Story Mode Mod Manager", font=ctk.CTkFont(size=14)).pack(pady=(20, 4))
        ctk.CTkLabel(self.content, text="Created by bunzy", font=ctk.CTkFont(size=13), text_color="gray").pack(pady=4)
        ctk.CTkLabel(self.content, text="Built with Python and CustomTkinter", font=ctk.CTkFont(size=12), text_color="gray").pack(pady=4)
        ctk.CTkLabel(self.content, text="Not affiliated with Telltale Games.", font=ctk.CTkFont(size=11), text_color="#555").pack(pady=(20, 4))


app = StoryForgeApp()
