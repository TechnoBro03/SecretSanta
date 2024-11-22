# Secret Santa Assignment Program
This C# program is designed to automate the process of assigning Secret Santa participants, ensuring a fun and fair gift exchange experience. It's perfect for office parties, family gatherings, or any group looking to enjoy the Secret Santa tradition without the hassle of manually coordinating the assignments.

## Features
* **Flexible Participant Input**: Reads participant names from a text file.
* **Avoids Group Assignments**: Automatically ensures that groups do not get each other as their Secret Santa, maintaining the surprise element.
* **Previous Assignment Consideration**: Previous assignments are accounted for, and participants are ensured that they do not get the same person they had in the past.
* **Reciprocal Swaps**: Optional setting to prevent reciprocal swaps between two participants (Person A gives to Person B, so Person B won't give to Person A).
* **Randomized Assignment**: Randomized assignment of Secret Santa, adding an element of unpredictability and fun.
* **Simple and User-Friendly**: Designed with simplicity in mind, making it easy to use even for those with no programming experience.
* **Nicely formatted output**.

## Usage
* Prepare your text files
  * ```namesFile```: Names are separated by commas. Groups are separated by new lines. See [names.txt](https://github.com/TechnoBro03/SecretSanta/blob/main/names.txt) for example.
  * ```assignmentsFile```: Optionally, previous assignments. 'Giver -> Recipient' separated by new lines. See [assignments.txt](https://github.com/TechnoBro03/SecretSanta/blob/main/assignments.txt) for example.
* Run the program with ```dotnet run <namesFile> <assignmentsFile> <checkPreviousAssignments> <allowReciprocal> [verboseOutput]```
* Check the ```assignmentsFile``` for the assigned pairs.
