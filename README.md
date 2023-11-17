# Secret Santa Assignment Program
This C# program is designed to automate the process of assigning Secret Santa participants, ensuring a fun and fair gift exchange experience. It's perfect for office parties, family gatherings, or any group looking to enjoy the Secret Santa tradition without the hassle of manually coordinating the assignments.

## Features
* Flexible Participant Input: Reads participant names from text file. Supports both individual participants and groups (listed on the same line, separated by commas). Space friendly.
* Avoids Group Assignments: Automatically ensures that groups do not get each other as their Secret Santa, maintaining the surprise element.
* Previous Assignment Consideration: Option to read previous assignments and ensure that participants do not get the same person they had in the past.
* Prevents Direct Swaps: Includes a check to prevent direct swaps between two participants (Person A gives to Person B, so Person B won't give to Person A).
* Randomized Assignment: Randomized assignment of Secret Santas, adding an element of unpredictability and fun.
* Simple and User-Friendly: Designed with simplicity in mind, making it easy to use even for those with no programming experience.
* Nicely formatted output.
## Usage
* Prepare your text files ('names.txt' and 'assignments.txt') with participant names and, optionally, previous assignments (in the format 'Person A->Person B', seperated by new lines).
* Run the program, enter the options you prefer, and the Secret Santa assignments are generated.
* Check 'assignments.txt' for the assigned pairs.
