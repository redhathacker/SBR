Notes

The main function of this application is listening for new values on port 161 at the following IP addresses:
192.168.1.40
192.168.1.41
192.168.1.42
192.168.1.43
192.168.1.44
192.168.1.45
192.168.1.46
192.168.1.47

Located at these addresses are: DAEnetIP2 - SNMP Ethernet controller with 24 digital/analog I/O
see http://denkovi.com/ethernet-snmp-controller-web-server-24-digital-and-analog-io for specs.

The OIDs (Object Identifier) for each of these cards is the same. There is one OID for 
each channel and 1 OID to specify all channels in the port:

.1.3.6.1.4.1.19865.1.2.1.1.0 - Digital Port1 Channel 1
.1.3.6.1.4.1.19865.1.2.1.2.0 - Digital Port1 Channel 2
.1.3.6.1.4.1.19865.1.2.1.3.0 - Digital Port1 Channel 3
.1.3.6.1.4.1.19865.1.2.1.4.0 - Digital Port1 Channel 4
.1.3.6.1.4.1.19865.1.2.1.5.0 - Digital Port1 Channel 5
.1.3.6.1.4.1.19865.1.2.1.6.0 - Digital Port1 Channel 6
.1.3.6.1.4.1.19865.1.2.1.7.0 - Digital Port1 Channel 7
.1.3.6.1.4.1.19865.1.2.1.8.0 - Digital Port1 Channel 8
.1.3.6.1.4.1.19865.1.2.1.33.0 - Digital Port1 All Channels

.1.3.6.1.4.1.19865.1.2.2.1.0 - Digital Port2 Channel 1
.1.3.6.1.4.1.19865.1.2.2.2.0 - Digital Port2 Channel 2
.1.3.6.1.4.1.19865.1.2.2.3.0 - Digital Port2 Channel 3
.1.3.6.1.4.1.19865.1.2.2.4.0 - Digital Port2 Channel 4
.1.3.6.1.4.1.19865.1.2.2.5.0 - Digital Port2 Channel 5
.1.3.6.1.4.1.19865.1.2.2.6.0 - Digital Port2 Channel 6
.1.3.6.1.4.1.19865.1.2.2.7.0 - Digital Port2 Channel 7
.1.3.6.1.4.1.19865.1.2.2.8.0 - Digital Port2 Channel 8
.1.3.6.1.4.1.19865.1.2.2.33.0 - Digital Port2 All Channels

.1.3.6.1.4.1.19865.1.2.3.1.0 - Analog Port1 Channel 1
.1.3.6.1.4.1.19865.1.2.3.2.0 - Analog Port1 Channel 2
.1.3.6.1.4.1.19865.1.2.3.3.0 - Analog Port1 Channel 3
.1.3.6.1.4.1.19865.1.2.3.4.0 - Analog Port1 Channel 4
.1.3.6.1.4.1.19865.1.2.3.5.0 - Analog Port1 Channel 5 - NOTUSED
.1.3.6.1.4.1.19865.1.2.3.6.0 - Analog Port1 Channel 6 - NOTUSED
.1.3.6.1.4.1.19865.1.2.3.7.0 - Analog Port1 Channel 7 - NOTUSED
.1.3.6.1.4.1.19865.1.2.3.8.0 - Analog Port1 Channel 8 - NOTUSED
.1.3.6.1.4.1.19865.1.2.3.33.0 - Analog Port1 All Channels  - NOTUSED


The bottle neck on this app is now and will forever be the SNMP Get call (reading the channel). I did 
everything humanly possible to minimize this but honestly until new hardware is purchased it is as fast 
as its going to get. Just for kicks I searched for new Relay Boards. They are far and few between and I only 
found 1 type of board that supported anything beyonnd SNMP V1 ..... $450 each!!!!!! ... as of today (3/14/2016)

Organization
----------------------------------------------------------------------------------------------------------------
X. Some projects were removed. The only reason you need an external DLL is if the same functionality is needed 
in multiple applications. That makes 1 less dependency that can break since there is no external DLL file and 
it makes future development easier especially if its another developer. 
1. The GPIO Library project and DLL are gone. The needed code was incorporated into the SBR project.
This is now in the EthernetBoard and the PciBoard class files. 
2. The Snmplibrary project and DLL are gone. Only 2 function calls were being used the SNMP Set and Get 
functionality (reading and writing channel values). These functions are now built into the EthernetBoard objects.
2. The AsioHandler Library project and DLL are gone. This code now resides in a class called AsioHandler.
6. The MyCore, AnimationExtensions, and MarginSetter classes were moved into their own class files. They were just 
in random places before that. I know its all the same Namespace but cmon man.....
X. A Channel class was created from scratch, it used to be inexorably tied to the Ports class. I understand why it was 
done this way but this is just bad design and I needed to create accompanying classes for (Boards, Ports and Channels) 
and that EthernetBoard class file was getting huge.
X. Regions were added to a lot of the code files (# region and # endregion) so that entire sections of code can 
now be collapsed. This is really just for anyone reading the code as it breaks it up into logical sections.
X. There were almost no comments anywhere. I have added a bunch. If you are another developer, you're welcome. 
You owe me a steak.
X. All of the yucky commented out code has been removed. It was mostly older functionality that had been redesigned.
There were vast swaths of commented code and huge gaps of whitespace in a bunch of the files. Unnecessary commented 
code is supposed to be removed before release versions are built. Commented code actually slows the compiler. 
Not much but still... If you didn't learn this in college, your teacher sucked.
3. The Boards, Ports and Channels classes were moved into their own code files. Classes now exist for the handling 
of the Channels, Ports and EthernetBoards. Each has a separate class for dealing with a collection of these 
objects, and a separate class for enumeration of these collections. 
4. All XML files were moved into a folder called Schemas. Any conflicts were resolved. In particular, the 
configuration.xml file was renamed to Appconfig.xml because configuration is already used by the CLR. 
5. XSD files were generated to accompany these XML files. I got tired of Visual Studio whining about the lack of
XML definitions so I was like.. fine here you go they will never change, here they are, thank you, shut up please. 
I havent heard a word from the compiler since. These you can find in the XSD (XML Schema Definition) folder.

----------------------------------------------------------------------------------------------------------------
Optimization

1. Some classes have been declared as sealed. This means that they cannont be inherited as the base of another class. 
Classes declared using the "sealed" keyword offer a small amount of optimization. 
2. Constant variables, Readonly variables, enums, and structs on values that never change were added. These constructs 
are pre-compied making them much leaner than an equivalent class or variable. This allows the precompiler to deal 
with them instead of the JIT compiler. This speeds up execution.
3. Since we are dealing with a multi-threaded design, everything that could be safely made into a static type 
was. Several of the classes do not actually hold data beyond the single execution of a method so these were made static. 
In addition to easing the development of this multi-threaded beast, static objects and variables are also mostly 
precompiled again speeding up execution. It is now faster than a kangaroo with its tail on fire!
4. For right now all logging was removed. Do we really need to know every time an SNMP call times out? Loggin is 
silly for single user applications anyway.
5. The entire application has been brought up to the 4.5 Framework. 
6. The entire application is now CIS compliant. Delegates with weird signatures were removed and new delegates
that accept (Object sender and EventArgs e) were added. Namespace conflicts now have explicit declarations. 
7. Global variables have been localized as much as possible.
------------------------------------------------------------------------------------------------------------------------
Threading Model

Omg dont get me started the threading in this thing was a friggin nightmare. I am now 5 years older and shitload smarter 
than I was. This was done to fix a delay between the time a button is pushed on the controller panel and its cooresponding 
light changing on the light panel. First anything that didn't need to be in the UI thread was moved. Running background 
tasks in a GUI thread is notoriously slow (Shame on you Geo) this is threading 101.  

Cancellation tokens
Tasks not Threads
Use Task.Run and not Task.Factory.StartNew for "fire and forget" methods 
Async methods
Task Parallel Library (TPL) where able


------------------------------------------------------------------------------------------------------------------------

Interfaces
------------------------------------------------------------------------------------------------------------------------
Some classes implement the IEnumerable, IEnumerator, ICollection and IDisposable interfaces. This lends functionality
of the interface to the class thereby making them scalable and saving coding time.

When implementing Interfaces you must define the methods required by the interface. You can do this either
implicitly or explicitly. Often they are interchangable. However in this project, I have used Implicit because 
I just dont like generic object type as it requires the compiler to do more work at run-time. I also used Implicit 
because I do not anticipate any of these classes being inherited or used as interface templates. Syntax below.

// Implicit IEnumerator methods
public object Current { get { throw new NotImplementedException(); } }
public bool MoveNext() { throw new NotImplementedException(); }
public void Reset() { throw new NotImplementedException(); }

// Explicit IEnumerator methods
object IEnumerator.Current { throw new NotImplementedException(); }
bool IEnumerator.MoveNext() { throw new NotImplementedException(); }
void IEnumerator.Reset() { throw new NotImplementedException(); }

// Explicit IEnumerator<T>
object IEnumerator.Current()
void IDisposable.Dispose()
bool IEnumerator.MoveNext()
void IEnumerator.Reset()

// Implicit IEnumerator<T>
public Channel Current
object IEnumerator.Current
public void Dispose() { }
----------------------------------------------------------------------------------------------------------------

Garbage Collection
------------------------------------------------------------------------------------------------------------------------
This is done by implementing the IDisposable interface. Dispose methods have been created when needed for cleaner release
of memory. Dispose methods are only needed if your class contains Disposable objects or unmanaged (code you didnt write) resources.
If your object holds references to IDisposable objects, then call Dispose() on these objects in the Dispose method.
if your object holds unmanaged resources, clean them up in the finalizer without re-writing any of the cleanup 
code in the Dispose(bool) method already. Dont build if you dont need. Its a waste of resources to have Dispose methods 
that are not needed.
----------------------------------------------------------------------------------------------------------------
Each class Now consists of the primary declaration class, a collection class, an enumeration class, and a static helper class
the will eventually be a threading handler as well. 

1. primary declaration class - This is pretty much the original class (Channel, Port , Board) It is the definition of the class and 
nothing else. It has 1 job and does it well, be a channel. 
2. The collection class - This is the definition of a collection object. These are special collection objects that were built for
dealing with a multi threaded environment. They handle the various collections (Channel lists, PortList, BoardList, etc)
3. 

