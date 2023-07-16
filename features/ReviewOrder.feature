Feature: Review Order


Scenario: Place Order without signing in
Given I am at the review order screen
And I am not signed in
When I fill the address bar
And I click the place order button
Then it should show a sign in screen
When I sign in
Then it should show order tracking screen

