Film files are saved to `C:\Users\YOUR_USER_PROFILE\AppData\LocalLow\MCC\Temporary\HaloReach\autosave` after completing or exiting out of campaign missions, they are then moved to `C:\Users\YOUR_USER_PROFILE\AppData\LocalLow\MCC\Temporary\UserContent\HaloReach\Movie` after you open the theater tab in the main menu. A maximum of 12 files are saved before the oldest files are written over. Film files will either have a `.film` or `.mov` extension. 

**PRIVACY NOTE**: Reach film files at the time this was written contain personally identifying information such as your Computer Name, Windows Account Name, Xbox Live account names, and possibly more. If you do not want someone to see this information then do not share your film files publicly. Microsoft will likely strip identifying information when theater is officially released, but until then you have been warned.

Looking at the file in a hex editor, these are the interesting data that I've found so far:

Data | Offset | Length | Info
---- | ------ | ------ | ----
Live account that created the file | 0x88 and 0xAC | 20 Bytes | ASCII
Mission Name | 0xC0 | 256 Bytes | Unicode
Mission Full Description | 0x1C0 | 256 Bytes | Unicode
Player Names | (1) 0x1E228, (2) 0x1E330, (3) 0x1E438, (4) 0x1E540 | 32 Bytes | Unicode
Player Clan Tags | (1) 0x1E26, (2) 0x1E374, (3) 0x1E47C, (4) 0x1E584 | 6 Bytes | Unicode
`ssig` and `flmd` Tags | 0x1F760 | Variable | Contains initial save and film data

If you play on a resumed save, the `flmd` tag will contain a checkpoint along with the tick inputs. I haven't gotten far enough to figure out all the header offsets so good luck.

I am just scanning through the header until finding a sequence of bytes, either `0x2B 0x80 0x00` or `0x02 0xB8 0x00`. Directly after this byte sequence starts the per-tick data, which includes player actions, rotations, and other data necessary to play the same simulation every time. The data layout here looks like this (sorry for bad drawing): 

![](https://i.imgur.com/kAovefI.png)

You will eventually run into a segment with "eof" which is the end of the film data. 

The tick data in each segment is bit packed and mostly unknown at this point how it is laid out. It is possible to determine what a couple of fields may be by starting a mission and performing very simple actions such as looking directly up/down, jumping, etc with no other inputs and then quitting out and looking at the data. So far I have identified that each player has bits for binary actions (jumping, meleeing, etc) as well as integer ranges for player rotation (11-bits for vertical, 13-bits for horizontal). Endianness may be flipped depending on some values so you'll have to check both ways. 
