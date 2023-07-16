Feature: Login

Scenario: Login Screen valid input
Given I am at login screen
When I typed a valid email address
Then it should ask me verification code

Scenario: Login Screen valid input2
Given I am at login screen
When I typed a valid email address
Then it should ask me verification code