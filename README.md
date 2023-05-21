# RailDriver

This mod adds RailDriver integration into Derail Valley

Instructions and details can be found in [the help site](https://help.ayra.ch/raildriver).
There you find documentation about the mod as well as the key map,
and at the bottom some extra information on the RailDriver device itself.

**The linked documentation is what you want as a user**

This readme file here is intended for developers.
It explains how you extend the mod.

# Extending the Mod

This mod allows pretty much limitless customization.
This includes:

- Taking over the display and showing custom content, and optionally reverting it again
- Changing values of analog controls before they're passed to the internal processing function
- Disable the internal processing function entirely
- Mapping custom code to any button or analog control
- Remapping buttons, incuding those used for internal functions

## How to Extend the Mod

*Note: This assumes you know the basics about Unity modding.*
*If you don't,*
*[click here](https://wiki.nexusmods.com/index.php/How_to_create_mod_for_unity_game)*
*or look at the "Test Mod" project.*

**TL;DR:** Start by fiddling around with the TestMod first (see below),
and come back here if you have problems or want more information.

----

To access the RailDriver mod,
add `DVRailDriverMod.Interface.dll` as a reference to your project.
You find the DLL as part of the main mod,
but also as an individual download in [the releases section](https://github.com/AyrA/DVRailDriverMod/releases).

Use `DVRailDriverMod.Interface.RailDriverBase.Instance` to get access to the mod
and interact with it. In most cases you want to attach to the `RailDriverInputChange` event.

In your `info.json` file, add the rail driver mod as dependency.

Example file contents:

```json
{
	"Id": "CHANGE_ME_TO_SOMETHING_UNIQUE",
	"DisplayName": "Change me to the name you want to be shown in the mod manager UI in the game",
	"Author": "Your name here",
	"Version": "1.0.0",
	"ManagerVersion": "0.22.0",
	"GameVersion": "1.0.0",
	"Requirements": ["AyrA.DVRailDriverMod"],
	"LoadAfter": ["AyrA.DVRailDriverMod"],
	"AssemblyName": "YourDllFile.dll",
	"EntryMethod": "YourNamespace.Main.Load"
}
```

### "Requirements" vs "LoadAfter"

You can specify `AyrA.DVRailDriverMod` in either the `Requirements` or the `LoadAfter` field.
The first one should be used if the dependency is mandatory,
and the second one if it's optional.
Using both (as seen in the example above) is identical to using just `Requirements`.

## Checking for Optional Mod

If the raildriver mod is optional
you have multiple ways of checking if it exists.
One common way is to check if the type itself exists:

```C#
var HasMod = Type.GetType("DVRailDriverMod.Interface.RailDriverBase") != null;
```

The advantage of doing it this way is that
you don't have to ship the `DVRailDriverMod.Interface.dll` with your mod.

If you do want to ship the DLL, then you can directly check for the instance:

```C#
var HasMod = DVRailDriverMod.Interface.RailDriverBase.Instance != null;
```

## Using the RailDriverBase Instance

The RailDriverBase instance exposes all functionality you need.
All RailDriver interaction will happen through this class.

Below are the methods and properties it exposes.

### Property `static RailDriverBase Instance`

This gives you global access to the mod.
Whenever the mod is loaded, this will be non-null.

If it's null the mod failed to load, or doesn't exists.
If the mod is installed but refuses to load, it is either disabled, or no RailDriver device was found.

Try using the calibration utility from the latest release to check if that works.
If that won't work either, your device is probably malfunctioning.
Try reconnecting it, or in extreme cases, restart your computer.

Note: The instance property can also null if your mod loaded first,
which can happen if you don't declare the dependency in the json file correctly.

### Event `RailDriverInputChangeEventHandler RailDriverInputChange`

This event is fired whenever the RailDriver status changes.
This is where you react to input changes as well as changing mod behavior.
The test mod (see further below) serves as an example for this.

The event has two arguments. The first one is the triggering instance,
and the second one a type that contains all event relevant information,
most importantly, the state of all buttons and levers.

In this event you can not only read the inputs, but also change them.
Any changes you make to the second event argument
will be propagated to other mods in the event chain,
and finally to the RailDriver mod itself.
The test project (see further down) demonstrates a few ways this can be used.

Note: Ensure whatever you do happens quickly.
Doing long running tasks can overflow the hardware buffer
and slows down other mods that want to interact with the RailDriver controller.
Create a thread for potentially long running tasks,
and make said thread deal with skipping event calls if it's too slow to keep up with them.

The `RailDriverInputChange` event already fires on a different thread.
This means a hanging mod should not freeze the main game loop,
it at worst renders the RailDriver inoperable.
It has no impact on other control methods (keyboard, mouse, VR, etc.)

Note: This event may be called multiple times in rapid succession.
If the user changes multiple controls at once it may fire multiple times,
each time with the appropriate ButtonType argument set,
but every invocation will already have all updated values.

The mod doesn't guarantees that there's a change in inputs between event calls.
This means the event may be fired even though the state of the controls
is identical to the previous state.

Also note that this event sometimes "misfires" because using some RailDriver controls
can slightly nudge other analog controls.
If this is a problem, you want to store the input state each time the event is called,
and check for sufficient change on successive calls.
For example you can check for a 10 percent change in the reverser lever
before you apply it to your locomotive and update the saved value.

### Property `bool IsDisplayInCustomMode`

This is automatically set to `true` if a mod has taken control of the display.
You can use `ResetDisplayBehavior()` to reset this to `false`
and return control to the RailDriverBase instance.

### Method `abstract void SetDisplayContents`

This method exists three times:

- One instance to set a number
- One instance to set text
- One instance to set LED segments directly

#### Using a number

The number version will automatically clamp and cut your number to make it fit the display.
In essence this means a range from -99.0 to +999.0,
and it will show one decimal place if the number is small enough to fit it on the display.

#### Using a string

The text version will automatically activate the scrolling behavior of the text display
if it's necessary to display the string.

Due to the limitation of a 7-segment display, the characters it can show are limited.
You can display all digits as well as all letters with the exception of `KMVWX`.
Additionally, the symbols `.,_-¯!?'°"()=@` can be shown.

Using unsupported characters in your string will replace them with a space.
The text is case insensitive.
Some letters look like numbers, so be careful when mixing numbers with letters,
and avoid confusing the user.

#### Using segment values

Using segment values will set the segments directly.
This can be used to draw custom symbols, or to draw animations.

The update speed of the display is fairly fast,
much faster than the device reports input changes actually,
but changing the display quickly can create annoying flickers,
and it's very distracting to the players.
In general, keep it under 5 changes per second.

### Method `void SetDisplayScrollDelay`

This sets the delay between text scroll steps.
By default it's 300 ms but you can adjust it any time.
Minimum duration is 100 ms, and increases in 100 ms steps.
Your argument is rounded down if it's not a multiple of 100,
and increased to 100 if it's smaller.

Using this function will not trigger custom display behavior.

### Method `void ResetDisplayBehavior`

This resets the display to the default behavior of the RailDriver mod.
It doesn't resets the scroll delay if you've changed that.

## The `RailDriverEventArgs` Type

This type is passed to the `RailDriverInputChange` event as the second argument.

To give other developers some consistency,
the properties are to be used as follows:

### Property `bool Handled`

If `true`, it instructs the base mod to not act on this event.
This should be set when your mod wants to do something
that otherwise conflicts with the base mod,
but you still want other mods to also be able to act on this event.

The default is `false`

### Property `bool Cancel`

If `true`, it instructs all other mods in the chain to not act on this event.
This is useful if your mod binds a function to a button
and doesn't wants other mods to also act on said button.
It also disables the base mod behavior (like the "Handled" property does)

The default is `false`

Note: A less intrusive way would be to simply mask the button out of the event,
but let other mods still access the event.
If the user presses multiple buttons at the same time,
this is sometimes only registered as one event
if the buttons are of the same `ButtonType`.
Masking your button instead of cancelling the event is a less intrusive way,
and more compatible with other mods.

Also note that this is on the honor system.
Other mods are not required to obey this, and can even revert this to `false`.

Don't expect a certain order of event handlers.

### Property `ButtonType ButtonType`

This is the button type that triggers the event.
Analog controls are their own "button",
but actual buttons are grouped.
The top left group is the "Aux" group,
the long button lines at the bottom are grouped into "TopRow" and "BottomRow",
the "UpDown" button is it's own type, and the D-Pad ("Cross") is it's own type.

### Property `RailDriverButtonValues ButtonValues`

This is the current state of all buttons and controls of the RailDriver.
You can not only read them but also change them to change the behavior
of other mods and the base mod.

Analog controls are provided with the raw and processed value.
Only the processed value is checked by the base mod,
and the raw value is seen as informational only.

Editing either the raw or processed value will not update the other.

The mod will not stop you from setting invalid analog values,
or making impossible button combinations.
In fact, the RailDriver device itself can generate invalid button combination,
most notably, the horn lever can be in the up and down position simultaneously,
if a few other specific buttons are pressed.
In general this means you want to code defensively and not blindly trust the values.

### Property `object Tag`

This propery can be freely changed by anyone.
This can be used to provide inter-mod communication inside of the event.
The value is not retained between event invocations.

## The Test Mod

This repository contains a "TestMod" project.
This is a fully functional extension that demonstrates how to change mod behavior.

Note: When you load it, you likely have to delete and re-add references,
because the paths are local to my system, and your installation likely differs.
Problematic references are shown with a yellow exclamation mark.

### Things this Mod Shows

The test mod showcases the most important things you might want to to.

#### Disabling Internal Functionality

This mod disables the ability to start/stop the engine using the leftmost button
on the **top** 14 button row.

#### New Button Action and Display Update

The two leftmost buttons in the **bottom** 14 button row will display "HELLO"
on the display, and reset it to the default behavior respectively.

### Changing Base Mod Behavior

The test mod limits the independent brakes power to 50%,
but still lets the base mod handle the processing of the input.

### Partially Disabling Functionality

This disables the emergency brake behavior,
but leaves the auto brake otherwise intact.
Moving the auto brake into the "EMG" position
is just normal full application of the brakes.

### Extending Functionality

This mod maps the "Bell" button to the horn lever.
Pressing the button will sound the train horn for as long as it's held down.

### Remapping Keys

This mod swaps the "Sand" and "Alert" keys.
