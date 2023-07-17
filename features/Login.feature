Feature: Login

Scenario: Login valid input
Given I was asked for login
When I provided a valid email address
Then it should ask me verification code

Scenario: Login valid input2
Given I was asked for login 
When I provided a valid email address
Then it should ask me verification code