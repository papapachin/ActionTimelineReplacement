# ActionTimelineReplacement aka.ATR

**Specially Credit to Loskh for the original development, and also very credit to DethiCra for completely rebuilding the plugin as well as creating the graphic interface!!**

This is a plugin that can simply replace the timeline(tmb) the casting VFX for skill invocation in the game. 
It can assist some mods in separating skills that originally use the same timeline(tmb) into using different timelines (tmb).
So as to achieve the effect of customizing the performances of these skills in cooperation with some mods. 
For example, the casting timeline of the Samurai's Iajitsu skill and the hit timelines of various  AOE skills.  

Open config window by `/atr`.


## How to use ?(for simple using)
If you are just an ordinary user rather than a modder, it is sufficient for you to check only steps 1 and 2. 
![S7(H@~XQLL%UW GG VV1}LF](https://github.com/user-attachments/assets/83cd283f-4838-4294-ad2b-efd801d386a0)

### 1. Install the plugin
By adding the repo link into your dalamud custom Plugin Repositories and search it in Plugin Installer.Then install it.
Repo link:
Global ver:https://raw.githubusercontent.com/papapachin/ActionTimelineReplacement/Global/repo.json

### 2. Open the plugin config window,and use "Import" to load the preset files which modders provided.
After the presets are imported, they are enabled by default. Each option can be toggled on and off independently.
Generally, any changes take effect immediately. If the changes don't take effect, you can try clicking the "Redraw" button at the top.

### 3. If you don't need one preset anymore,you can right click the preset name in the left list,and delete it.

## The following is for advanced using
Ordinary users with simple using should not randomly modify the presets. If you accidentally modify a certain preset and don't know how to restore it, please delete it and re-import the version provided by the author. 

### 4. Click "To Advanced Mode" to show the advanced editing interface
You can create a new preset by clicking "Create" botton at preset list area.And rename it by input at the preset config area.
Then you can create a replacing by clicking "+" or delete one replacing by click "-" at preset config area.
And change their casting vfx/start timeline/end timeline/hit timeline by editing their number.
You can use some plugins to find the corresponding relationship between the numbers and the timeline paths. Here, it is recommended to use "alpha".
https://github.com/NotNite/Alpha
FFXIV explorer and Godbert can also work for it.

### 5. Create and Edit your own preset.
If you finished the editing,right click the preset name at preset list,you can export it as a json file.

### 6. Export the preset file 
It will. I use CN client for developing & testing latest features first (because of the subscription), and I'll make another repo for distributing it for Intl clients. 

## Q&A

### 1. Will the modifications it makes be detected by the server?
These modifications by ATR are also local, and basically speaking, they are safe.

### 2. What's the difference between it and using mods to swap skills?
Mods swap the internal content of the tmb used by skills, but which tmb path a specific skill uses won't change. 
Therefore, when multiple skills use the same tmb, mods can do nothing even if they want to make these skills look different. In this case, ATR can be used.

### 3. Can the modifications made by ATR be synchronized through Mare?
Unfortunately, for now, the modifications of ATR cannot be synchronized through Mar
