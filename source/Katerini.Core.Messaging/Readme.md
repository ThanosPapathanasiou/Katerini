## Katerini.Core.Messaging

To keep things simple:

1. The message and message handler are in the same file.
2. The message, once in production can only change fields in a JSON backwards compatible way i.e. 
   1. You can add nullable fields but not mandatory ones.
   2. You cannot remove fields
   3. Unit tests should ensure compatibility
      1. JSON serialized message before the change should be able to be deserialized after the change

If compatibility cannot be ensured then it's time to
create a V2 version of the message instead of changing the existing one.

You can keep the V2 version of the message in the same file and 
you can even use the same handler by adding a second interface to it.

If you have been programming in C# using 'Clean Code' 
you will instinctually try to create a whole bunch of different files
instead of keeping messages and message handlers in the same file. 

_**Please try to fight that instinct.**_ 

Ask yourself, what is easier to reason with?
- 10 files that have 500 lines of code each?
- 125 files that have 40 lines of code each?

Both options have 5000 lines of code but one is much better to navigate, visualize and ultimately work with.