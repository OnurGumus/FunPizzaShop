# Building & Running


This is the source code for my Full Stack DDD, CQRS F# Workshop available here:

* July 17-21:

https://www.meetup.com/tackling-f-web-development/events/294016439/

* Aug 20-21, 26-27 (Weekends):

https://www.meetup.com/tackling-f-web-development/events/294016439/

Live Demo Online:


https://funpizzashop.azurewebsites.net/

![](https://raw.githubusercontent.com/OnurGumus/FunPizzaShop/main/funpizzashop.gif)


You need `Docker for Desktop` installed in your machine for MacOS or Windows. 
You also need `VSCode` with `Dev Containers` extensions.

Open the project with VSCode, you should see the `Reopen in Container` button from VSCode. Click the button and wait until container set up ends.

Once done, open a terminal in vscode and type

``` bash
./fake.sh
```

Then navigate to 

```
http://localhost:8000.
```


Sign in requires send grid key, to send verification code. If you don't have it, you can find the verification code from the logs in terminal. If you have it, you need to provide via asp.net config. I use a file called secrets.hocon, must be inside debug/bin folder of Server project when running with content:

```hocon
config{
    SendGrid.APIKEY = "<your_key>"
}
```

To build for production 

``` bash
./fake.sh PublishServer
```
Then grap the `deploy` folder
