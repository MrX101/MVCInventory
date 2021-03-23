
![Gif](https://imgur.com/a/d4Vpix6)


https://i.imgur.com/P97sI7w.png

This is an example project to show my coding skill. It is an inventory system coded in the MVC(Model, View, Controller) coding architecture. To ensure that the functionality is not effected by the Graphical User interface and as such any number of future changes to the GUI could be done and the functionality would not be effected.



If you want to run the local yourself but don’t have much/any Unity experience:
Install Unity Hub from https://unity3d.com/get-unity/download
You will need to make an account, if you don’t have one
After Installing, open the program, login then find 2019.4.19f in this website https://unity3d.com/get-unity/download/archive and click the UnityHub Button for that version, it should popup asking you to install with UnityHub.

Now back to your git application, after you clone it and download the files, run UnityHub > Go to “Projects” tab > Click “Add” Button in the topright > FInd the location where you cloned the repo > Click Select Folder > Now Double click the new Item that appeared in the “Projects” list to open the repo. It should take a few minutes to generate all the temporary files it needs.



If you are not familiar with Unity, please focus your attention on the Assets/Scripts folder. Those are the scripts I have made.

Please note that a monobehaviour is basically a GameObject that exists within a scene and calls numerous methods during runtime. 

Awake - Start of game loading or when object is created/enabled.
OnEnable - After Awake, shortly after the object is enabled/created.
Start - Is similar to Awake, but called after OnEnable.
Update - Called once everyframe after being created/enabled.
OnValidate - Called when values are changed in the Unity Editor.
OnDisable - Called when the object is disabled.
Also note that this is using the builtin Unity Test framework to allow Unit testing via NUnit. Though the Tests themselves are run in the Unity Editor via GUI.
https://i.imgur.com/M4hlObB.png
https://i.imgur.com/XpxtIxy.png

As can be seen in image  above. The code for the inventory is separated into the typical MVC style of Model(Data), View(GUI) and Controller.

The Core components of the Data are the IItem.cs, DataInventoryContainer.cs and DataInventorySlot.cs

https://i.imgur.com/T2BZJsZ.png
IItem is simply a basic interface to give us the core functionality of managing items and allow us the flexibility of making anything we might need in future,  into an item. Though for simplicity sake there is currently only the BaseItem inheriting from the IItem interface.


As should be obvious from their name, an inventory will have a number of Containers and each container will have a number of inventory slots, which might have a singular item stored.

Now in theory the Inventory Class itself might have also been in the Data Category, instead of the controller category, and I could have created a wrapper controller class around the Inventory class, to further abstract it. But it felt like I was basically doing the same code twice to some degree, so I went with the Inventory being the controller and the GUI directly interfacing with the inventory class for requests/updates. Though I might change this in future.

Please note the DataEquiptmentContainer inherits from the DataInventoryContainer, and overrides the StoreItem method, so that it also equips said item.

The WeaponWheelContainer is not yet implemented, but it would simply be an equipment slot with multiple equipped slots where only 1 item is currently selected as a primary, which would be quite useful in a game where you have a lot of equipped weapons and want to swap between them without constantly equipping/unequipping items(eg. Shooter games).

The GUI components have their respective data versions, the containers, slots, items and equipment Slots. All with GUI at the end of their class name.
The GlobalInventoryControllerGUI is what directly interacts with the Inventory class and makes requests and tells the other GUI components what to change/update.

The GUI InventorySlots and Items are using the Unity Interfaces for handling pointer clicks/movements/dragging (IDropHandler ,IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler). These interfaces are very simple and only contain a single method, that will be called when their events occur. Though they unfortunately don’t have the best hitbox detection with dropping items into the itemslots. So I might replace them in future with a version that can calculate the closest slot where the dragging stopped.

If you try to use any of the code, please make sure you understand how Unity’s assembly definitions work, as you won’t be able to simply use namepsaces without first declaring them in the assembly definition files(which are required when using Unit Tests in Unity) in the Unity Editor.

Currently missing features are:
Adding More Unit Tests
Add tooltips
Make the inventory functions return a response based enum, instead of success/fail bool.To indicate why the operation failed.
Adding Universal Item Manager that stores all items and other inspectors simply show the item list with a searchbox.
World items dropping/pickups. ~Partially implemented.
Destroying Items in GUI
Make a better inspector for setting inventories settings.


Major Features:
Add Network Support
Add Async requests/responses.
Add Database Support
