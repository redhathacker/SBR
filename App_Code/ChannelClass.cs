using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Lextm.SharpSnmpLib;
using System.Linq;
using System.Collections.Concurrent; // USE THESE COLLECTIONS WHEN DEALING WITH MUTIPLE THREADS

// arrays and lists are Ienumerable objects - 
// Get Index - ((IList<Channel>)__contents).IndexOf(value);
// Conv Array to Ienum -  IEnumerable<Channel> __RandomCh = __contents; 
// Convert List to Ienumerable IEnumerable<Channel> __ChannelColl = __lstChannels;

/// 
/// <summary>
/// Implementzations: IClonable (MemberWise), ICollection<T>, IEnumerator<T>, IEmumeration<T>, IEquatable<T>
/// This class can compare itself to another copy to see if they are equal .Equal() IEquatable interface
/// This class can make new instaces that are exact compies of itself. IClonable interface
/// Allows you to set the value of the channel
/// </summary>
namespace snmpd
{
     // DONE DONT TOUCH
     #region Class: Channel Definition
     public sealed class Channel : IEquatable<Channel>, ICloneable
     {
          // Public Properties
          public int Value { get; private set; }
          public ChannelState State { get; set; }
          public int Id { get; private set; }
          public string Name { get; private set; }
          public bool IsDirty { get; set; }

          /// <summary>
          /// This ObjectIdentifer holds the unique OID value of the present channel.
          /// </summary>
          public ObjectIdentifier OID { get; private set; }

          /// <summary>
          /// Has the Value changed but not been saved?
          /// </summary>

          // Channel Constructors, There is no empty constructor
          public Channel(int _id, string _oid, string _name, ChannelState _state)
          {
               Id = _id;
               OID = new ObjectIdentifier(_oid);
               Name = _name;
               State = _state;
               IsDirty = false;
               Value = _state == ChannelState.OFF ? 0 : 1;

          }
          public Channel(int _id, ObjectIdentifier _oid, string _name, ChannelState _state)
          {
               Id = _id;
               OID = _oid;
               Name = _name;
               State = _state;
               IsDirty = false;
               Value = _state == ChannelState.OFF ? 0 : 1;
          }

          /// <summary>
          ///  Pass in the ChannelState enum to update the Value of this channel.
          /// </summary>
          public void SetValue(ChannelState pState)
          {
               State = pState;

               if (pState == ChannelState.ON)
                    Value = 1;
               else
                    Value = 0;
          }

          /// <summary>
          /// Is some other channel identical to this one. 
          /// </summary>
          /// <param name="other">The other channel.</param>
          bool IEquatable<Channel>.Equals(Channel other)
          {
               if (OID == other.OID && Name == other.Name && Id == other.Id)
                    return true;

               return false;
          }

          #region ICloneable Definitions

          /// <summary>
          /// Create a brand new Channel instance that is a copy of this one. 
          /// </summary>
          /// <returns></returns>
          object ICloneable.Clone()
          {
               // Ha!! Turns out my code is better. Makes a true copy.
               // return new Channel(Id, OID, Name, State);
               // Microsofts method is to use MemberWise
               return DeepCopy();
          }

          /// <summary>
          /// Basically just copies the memory address. The copy will change values in the original variable;
          /// </summary>
          /// <returns></returns>
          private Channel ShallowCopy()
          {
               return (Channel)MemberwiseClone();
          }
          /// <summary>
          /// Copies everything structure and values into a new brand new memory location;
          /// </summary>
          /// <returns></returns>
          private Channel DeepCopy()
          {
               Channel other = (Channel)MemberwiseClone();
               other.OID = new ObjectIdentifier(this.OID.ToString());
               other.Name = String.Copy(Name);
               other.State = this.State;
               other.Id = this.Id;
               return other;
          }

          #endregion - end interface implementation

     }

     #endregion - End class

     #region Class: Channel(s) Definition 

     /// <summary>
     /// This class contains properties and methods for working with a collection of Channel objects.
     /// It implements many interfaces to do this: IEnumerable, IEnumerable<Channel>, ICollection, ICollection<Channel>, 
     /// almost all collections in .NET implement IEnumerable. 
     /// </summary>
     //public class Channels : ICollection, ICollection<Channel>, IList, IList<Channel>
     //{
     //     private BlockingCollection<Channel> __ThreadSafeChannels = new BlockingCollection<Channel>();

     //     // The base object is an array of type Channel
     //     private Channel[] __contents;
     //     private Channel[] __contents2;

     //     // lock object
     //     private readonly object __MySyncRootLock = new object();
     //     public bool IsArrayInitialized = false;

     //     public int GetLowerBound(int dimension) { return __contents.GetLowerBound(0); }
     //     public int GetUpperBound(int dimension) { return __contents.GetUpperBound(0); }

     //     // Constructors
     //     public Channels(Channel[] pArray)
     //     {
     //          lock (__MySyncRootLock) // Restrict Access
     //          {
     //               foreach (Channel ch in pArray)
     //               {
     //                    __ThreadSafeChannels.Add(ch);
     //               }

     //               __contents = pArray;
     //               __contents2.SelectMany(c => pArray);
     //               IsArrayInitialized = true;
     //          }
     //     }

     //     public Channels()
     //     {
     //          // start with empty array
     //          __contents = new Channel[0];
     //          __contents.CopyTo(__contents2, 0);
     //     }

     //     public Channel this[int index]
     //     {
     //          get { return ((IList<Channel>)__contents)[index]; }
     //          set { ((IList<Channel>)__contents)[index] = value; }
     //     }

     //     // ICollection - SyncRoot, IsReadOnly, IsSynchronized, Count
     //     bool ICollection<Channel>.IsReadOnly { get { return __contents.IsReadOnly; } }

     //     #region CopyTo Method declarations

     //     //********** CopyTo Methods ***************
     //     /// <summary>
     //     /// Copy the internal data collection into and array.
     //     /// </summary>
     //     /// <param name="pArray">The array to copy to.</param>
     //     /// <param name="index">The index to begin copying.</param>
     //     public void CopyTo(Array pArray, int index)
     //     {
     //          CheckForErrors(pArray, index);

     //          lock (__MySyncRootLock) // Restrict Access
     //          {
     //               Array.Copy(__contents, GetLowerBound(0), pArray, index, Count);
     //          }
     //     }

     //     /// <summary>
     //     /// Check the Array for various errors before copying.
     //     /// </summary>
     //     /// <param name="pArray">The array to copy to.</param>
     //     /// <param name="index">The index to begin copying.</param>
     //     private void CheckForErrors(Array pArray, int index)
     //     {
     //          string errParamName = "Channels.CopyTo(Array)";

     //          if (pArray == null)
     //               throw new ArgumentNullException(errParamName, "Null values are not allowed.");
     //          if (pArray != null && pArray.Rank != 1)
     //               throw new ArgumentException("MultiDimensional Arrays are not supported fool!!!!", errParamName);
     //          if (index < 0)
     //               throw new ArgumentOutOfRangeException(errParamName, "Index cannot be less than zero.");
     //          if (pArray.Length - index < Count)
     //               throw new ArgumentException("The Begin index cannot be greater than the number of elements in the array foo!!!.", errParamName);
     //     }

     //     /// <summary>
     //     /// Check the Collection for various errors before copying.
     //     /// </summary>
     //     /// <param name="c">The Collection to copy to.</param>
     //     /// <param name="index">The index to begin copying.</param>
     //     private void CheckForErrors(ICollection pCollection, int index)
     //     {
     //          if (index < 0 || (pCollection.Count - index) < 0)
     //               throw new ArgumentOutOfRangeException("CheckForErrors(ICollection)", "Index cannot be less than zero.");
     //          if (pCollection == null)
     //               throw new ArgumentNullException("CheckForErrors(ICollection)", "Null values are not allowed.");
     //          if (pCollection.Count - index < Count)
     //               throw new ArgumentException("The number of elements in the source CollectionBase is greater than the available space from index to the end of the destination array.");
     //          if (pCollection.OfType<Channel>().GetType().GetElementType() != typeof(Channel))
     //               throw new InvalidCastException("Not all of the elemnts in this collection are of Type Channel.");
     //     }

     //     /// <summary>
     //     /// Copy the internal Data collection into a collection of Channel objects.
     //     /// Append to the end of the List.
     //     /// </summary>
     //     /// <param name="pCollection">The collection to check for errors.</param>
     //     public void CopyTo(ICollection<Channel> pCollection)
     //     {
     //          if (pCollection.IsReadOnly)
     //               throw new ArgumentException("CopyTo(ICollection<Channel>)", "The collection is ReadOnly. Cmon man!!!");
     //          if (pCollection == null)
     //               throw new ArgumentNullException("CopyTo(ICollection<Channel>)", "Null values are not allowed.");
     //          if (pCollection.Any().GetType().GetElementType() != typeof(Channel))
     //               throw new ArgumentException("The number of elements in the source CollectionBase is greater than the available space from index to the end of the destination array.");

     //          lock (__MySyncRootLock) // Restrict Access
     //          {
     //               pCollection.SelectMany(c => __contents);
     //          }
     //     }

     //     /// <summary>
     //     /// Copy the internal data collection into an array of channels.
     //     /// </summary>
     //     /// <param name="pArray">The array to copy to.</param>
     //     /// <param name="pStartIndex">The index to begin copying.</param>
     //     public void CopyTo(Channel[] pArray, int pStartIndex)
     //     {
     //          try
     //          {
     //               CheckForErrors(pArray, pStartIndex);
     //               int stopCount = pStartIndex + Count;

     //               lock (__MySyncRootLock) // Restrict Access
     //               {
     //                    for (int i = 0; i < Count; i++)
     //                    {
     //                         pArray[pStartIndex + 1] = __contents[i];
     //                    }
     //               } // End Lock
     //          }
     //          catch
     //          {
     //               throw;
     //          }
     //     }

     //     /// <summary>
     //     /// Copy all the elements in the internal data collection into a List of Channels.
     //     /// Appends to the end of the List.
     //     /// </summary>
     //     /// <param name="lstChan">The List to copy to.</param>
     //     public void CopyTo(List<Channel> lstChan)
     //     {
     //          lock (__MySyncRootLock)
     //          {
     //               lstChan.AddRange(__contents.ToList());
     //          } // End Lock
     //     }

     //     /// <summary>
     //     /// Copy the internal data ICollection object into an array.
     //     /// </summary>
     //     /// <param name="pArray">The array to copy to.</param>
     //     /// <param name="pStartIndex">The index at which to begin copying.</param>
     //     void ICollection.CopyTo(Array pArray, int pStartIndex)
     //     {
     //          CheckForErrors(pArray, pStartIndex);

     //          lock (__MySyncRootLock) // Restrict Access
     //          {
     //               __contents.CopyTo(pArray, pStartIndex);
     //          } // End Lock
     //     }

     //     #endregion - CopyTo


     //     public void Clear()
     //     {
     //          for (int i = __contents.Length; i >= 0; i--)
     //          {
     //               Remove(__contents[i]);
     //               Remove(__contents2[i]);
     //          }

     //          IsArrayInitialized = false;
     //     }


     //     #region IList definitions

     //     object IList.this[int index]
     //     {
     //          get { return ((IList)__contents)[index]; }
     //          set { ((IList)__contents)[index] = value; }
     //     }

     //     public int IndexOf(Channel item) { return ((IList<Channel>)__contents).IndexOf(item); }
     //     public void Insert(int index, Channel item) { ((IList<Channel>)__contents).Insert(index, item); }

     //     IEnumerator<Channel> IEnumerable<Channel>.GetEnumerator()
     //     {
     //          return ((ICollection<Channel>)__contents).GetEnumerator();
     //     }

     //     #endregion - iList DEfinitions
     //}


     #endregion - End Class Channels

     public class ChannelsCollection : IEnumerable<Channel>, ICollection<Channel>
     {
          private Channel[] __contents;
          int _elementCount = -1;

          public ChannelsCollection()
          {
               __contents = new Channel[8];
          }

          public ChannelsCollection(Channel[] _chArry)
          {
               __contents = _chArry;
               _elementCount = __contents.Length;
          }

          public int Count { get { return _elementCount; } }
          public bool IsReadOnly { get { return ((ICollection<Channel>)__contents).IsReadOnly; } }
          public void Add(Channel item) { ((ICollection<Channel>)__contents).Add(item); }
          public void Clear() { ((ICollection<Channel>)__contents).Clear(); }
          public bool Contains(Channel item) { return ((ICollection<Channel>)__contents).Contains(item); }

          public void CopyTo(Channel[] array, int arrayIndex)
          {
               string errParamName = "ChannelsCollection.CopyTo(Array)";

               if (array == null)
                    throw new ArgumentNullException(errParamName, "Null values are not allowed.");
               if (array != null && array.Rank != 1)
                    throw new ArgumentException("MultiDimensional Arrays are not supported fool!!!!", errParamName);
               if (arrayIndex < 0)
                    throw new ArgumentOutOfRangeException(errParamName, "Index cannot be less than zero.");
               if (array.Length - arrayIndex < Count)
                    throw new ArgumentException("The Begin index cannot be greater than the number of elements in the array foo!!!.", errParamName);

               ((ICollection<Channel>)__contents).CopyTo(array, arrayIndex);
          }

          public IEnumerator<Channel> GetEnumerator()
          {
               return ((IEnumerable<Channel>)__contents).GetEnumerator();
          }

          public bool Remove(Channel item)
          {
               return ((ICollection<Channel>)__contents).Remove(item);
          }

          IEnumerator IEnumerable.GetEnumerator()
          {
               return ((IEnumerable<Channel>)__contents).GetEnumerator();
          }
     }

     #region ChannelsEnumerator Class

     public sealed class ChannelsEnumerator : IEnumerator<Channel>
     {
          ChannelsCollection __collection;
          ChannelsCollection __originalCollection;
          int position = -1;

          public int CurrentIndex { get { return position; } }

          public ChannelsEnumerator(ChannelsCollection ChCol)
          {
               __collection = ChCol;
               __originalCollection = ChCol;
          }

          public Channel Current { get { return __collection.ElementAt(position); } }          
          object IEnumerator.Current { get { return __collection.ElementAt(position); } }

          public void Dispose()
          {
               __collection = null;
               __originalCollection = null;
               this.Dispose(true);
          }

          public void Dispose(bool s)
          {
               
          }

          public bool MoveNext()
          {
               if (position < __collection.Count)
               {
                    position++;
                    return true;
               }
               else
               {
                    return false;
               }
          }
          public void Reset()
          {
               position = -1;
          }

     }

#endregion

#region ChannelController class

     /// <summary>
     /// This class is used to manage access to the channels. 
     /// </summary>
     public static class ChannelHelper
     {
          public static readonly ObjectIdentifier Io1_Ch1 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1.1.0");
          public static readonly ObjectIdentifier Io1_Ch2 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1.2.0");
          public static readonly ObjectIdentifier Io1_Ch3 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1.3.0");
          public static readonly ObjectIdentifier Io1_Ch4 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1.4.0");
          public static readonly ObjectIdentifier Io1_Ch5 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1.5.0");
          public static readonly ObjectIdentifier Io1_Ch6 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1.6.0");
          public static readonly ObjectIdentifier Io1_Ch7 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1.7.0");
          public static readonly ObjectIdentifier Io1_Ch8 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1.8.0");
          public static readonly ObjectIdentifier Io1_All = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.1.33.0");

          public static readonly ObjectIdentifier Io2_Ch1 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2.1.0");
          public static readonly ObjectIdentifier Io2_Ch2 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2.2.0");
          public static readonly ObjectIdentifier Io2_Ch3 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2.3.0");
          public static readonly ObjectIdentifier Io2_Ch4 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2.4.0");
          public static readonly ObjectIdentifier Io2_Ch5 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2.5.0");
          public static readonly ObjectIdentifier Io2_Ch6 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2.6.0");
          public static readonly ObjectIdentifier Io2_Ch7 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2.7.0");
          public static readonly ObjectIdentifier Io2_Ch8 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2.8.0");
          public static readonly ObjectIdentifier Io2_All = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.2.33.0");

          public static readonly ObjectIdentifier Ad1_Ch1 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.3.1.0");
          public static readonly ObjectIdentifier Ad1_Ch2 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.3.2.0");
          public static readonly ObjectIdentifier Ad1_Ch3 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.4.3.0");
          public static readonly ObjectIdentifier Ad1_Ch4 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.3.4.0");
          public static readonly ObjectIdentifier Ad1_Ch5 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.3.5.0");
          public static readonly ObjectIdentifier Ad1_Ch6 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.3.6.0");
          public static readonly ObjectIdentifier Ad1_Ch7 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.4.7.0");
          public static readonly ObjectIdentifier Ad1_Ch8 = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.3.8.0");
          public static readonly ObjectIdentifier Ad1_All = new ObjectIdentifier(".1.3.6.1.4.1.19865.1.2.3.33.0");

          public static Channel ch1_1 = new Channel(1, Io1_Ch1, "Chan1_Io1", 0);
          public static Channel ch1_2 = new Channel(2, Io1_Ch2, "Chan2_Io1", 0);
          public static Channel ch1_3 = new Channel(3, Io1_Ch3, "Chan3_Io1", 0);
          public static Channel ch1_4 = new Channel(4, Io1_Ch4, "Chan4_Io1", 0);
          public static Channel ch1_5 = new Channel(5, Io1_Ch5, "Chan5_Io1", 0);
          public static Channel ch1_6 = new Channel(6, Io1_Ch6, "Chan6_Io1", 0);
          public static Channel ch1_7 = new Channel(7, Io1_Ch7, "Chan7_Io1", 0);
          public static Channel ch1_8 = new Channel(8, Io1_Ch8, "Chan8_Io1", 0);
          public static Channel ch1_33 = new Channel(33, Io1_All, "ChanAll_Io1", 0);

          public static Channel ch2_1 = new Channel(1, Io2_Ch1, "Chan1_Io2", 0);
          public static Channel ch2_2 = new Channel(2, Io2_Ch2, "Chan2_Io2", 0);
          public static Channel ch2_3 = new Channel(3, Io2_Ch3, "Chan3_Io2", 0);
          public static Channel ch2_4 = new Channel(4, Io2_Ch4, "Chan4_Io2", 0);
          public static Channel ch2_5 = new Channel(5, Io2_Ch5, "Chan5_Io2", 0);
          public static Channel ch2_6 = new Channel(6, Io2_Ch6, "Chan6_Io2", 0);
          public static Channel ch2_7 = new Channel(7, Io2_Ch7, "Chan7_Io2", 0);
          public static Channel ch2_8 = new Channel(8, Io2_Ch8, "Chan8_Io2", 0);
          public static Channel ch2_33 = new Channel(33, Io2_All, "ChanAll_Io2", 0);

          public static Channel ch3_1 = new Channel(1, Ad1_Ch1, "Chan1_Ad1", 0);
          public static Channel ch3_2 = new Channel(2, Ad1_Ch2, "Chan2_Ad1", 0);
          public static Channel ch3_3 = new Channel(3, Ad1_Ch3, "Chan3_Ad1", 0);
          public static Channel ch3_4 = new Channel(4, Ad1_Ch4, "Chan4_Ad1", 0);
          public static Channel ch3_5 = new Channel(5, Ad1_Ch1, "Chan5_Ad1", 0);
          public static Channel ch3_6 = new Channel(6, Ad1_Ch2, "Chan6_Ad1", 0);
          public static Channel ch3_7 = new Channel(7, Ad1_Ch3, "Chan7_Ad1", 0);
          public static Channel ch3_8 = new Channel(8, Ad1_Ch4, "Chan8_Ad1", 0);
          public static Channel ch3_33 = new Channel(33, Ad1_All, "ChanAll_Ad1", 0);

          public static readonly List<Channel> AllChannels = new List<Channel>
          { ch1_1, ch1_2, ch1_3, ch1_4, ch1_5, ch1_6, ch1_7, ch1_8,
          ch2_1, ch2_2, ch2_3, ch2_4, ch2_5, ch2_6, ch2_7, ch2_8,
          ch3_1, ch3_2, ch3_3, ch3_4, ch3_5, ch3_6, ch3_7, ch3_8 };

          public static readonly List<Channel> AllDigitalBoard1Channels = new List<Channel>
          { ch1_1, ch1_2, ch1_3, ch1_4, ch1_5, ch1_6, ch1_7, ch1_8 };

          public static readonly List<Channel> AllDigitalBoard2Channels = new List<Channel>
          { ch2_1, ch2_2, ch2_3, ch2_4, ch2_5, ch2_6, ch2_7, ch2_8 };

          public static readonly List<Channel> AllAnalogBoard1Channels = new List<Channel>
          { ch3_1, ch3_2, ch3_3, ch3_4, ch3_5, ch3_6, ch3_7, ch3_8 };

          private const int TotalNumberOfChannels = 24;

          // public properties
          //public List<Channel> ChannelsList { get { return _lstChannels; } }
          //public Channel Current
          //{
          //     get
          //     {
          //          try
          //          {
          //               return _lstChannels[CurrentIndex];
          //          }
          //          catch (IndexOutOfRangeException)
          //          {
          //               throw new InvalidOperationException();
          //          }
          //     }
          //}

          //#region Controller Type methods (Add, Get, Set, etc.)

          ///// <summary>
          ///// Returns a Channel object.
          ///// </summary>
          ///// <param name="Oid">The Object Identifier of the channel.</param>
          ///// <returns></returns>
          //public Channel GetChannel(string Oid)
          //{
          //     return _lstChannels.Find(c => c.OID.ToString() == Oid);
          //}

          ///// <summary>
          ///// Returns a Channel object.
          ///// </summary>
          ///// <param name="BoardId">The ID of the Board.</param>
          ///// <param name="ChannelNo">The Channel Number.</param>
          ///// <returns></returns>
          //private Channel GetChannel(EthernetBoardPortNo IoPortNo, int ChannelNo)
          //{
          //     if (IoPortNo == EthernetBoardPortNo.DIGITAL_PORT_1)
          //     {
          //          return _lstDigitalBoard1Channels.Find(c => c.Id == ChannelNo);
          //     }
          //     else if (IoPortNo == EthernetBoardPortNo.ADC_PORT_1)
          //     {
          //          return _lstAnalogBoard1Channels.Find(c => c.Id == ChannelNo);
          //     }
          //     else // Default is Port 2 - most common
          //     {
          //          return _lstDigitalBoard2Channels.Find(c => c.Id == ChannelNo);
          //     }
          //}

          ///// <summary>
          ///// Returns an IEnumerable object that contains all of the channel objects belonging to the specified board.
          ///// </summary>
          ///// <param name="BoardId">The ID of an EthernetBoard.</param>
          //public IEnumerable<Channel> GetBoardChannels(EthernetBoardPortNo IoPortNo)
          //{
          //     if (IoPortNo == EthernetBoardPortNo.DIGITAL_PORT_1)
          //     {
          //          return _lstDigitalBoard1Channels;
          //     }
          //     else if (IoPortNo == EthernetBoardPortNo.ADC_PORT_1)
          //     {
          //          return _lstAnalogBoard1Channels;
          //     }
          //     else // Default is Port 2 - most common
          //     {
          //          return _lstDigitalBoard2Channels;
          //     }
          //}

          ///// <summary>
          ///// Returns an IEnumerable object of Channel objects belonging to the specified board.
          ///// </summary>
          //public IEnumerable<Channel> GetAllBoardChannels()
          //{
          //     return _lstChannels;
          //}

          //#endregion
     }

     #endregion
}
