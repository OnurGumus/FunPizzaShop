# Testimonials for this workshop

"The workshop was a condensed grouping of simple but very powerful  modeling techniques. We could see how to wire complex Tecnologies in a meaningful and flexible architecture, that's built for scaling from the ground up while we still maintaining the code semantics. That will highly impact my future arquitetural decisions for sure."
Jessé Junior

---
"I attended a brilliant workshop hosted by Onur where he covered a huge amount of material including CQRS/ES architecture, back-end & front-end FSharp development stacks."

Darran, Principal Software Developer

---

"The content of this course was very interesting: Onur has brought disparate F# technologies together and made a promising web stack from them. There's plenty for a SAFE Stack enthusiast like me to explore further!" 

Matt Gallagher, Consultant, Compositional IT


# Building & Running


This is the source code for my Full Stack DDD, CQRS F# Workshop available here:

* July 17-21:

https://www.meetup.com/tackling-f-web-development/events/294016439/

* Aug 20-21, 26-27 (Weekends):

https://www.meetup.com/tackling-f-web-development/events/294016439/



![](https://raw.githubusercontent.com/OnurGumus/FunPizzaShop/main/funpizzashop.gif)


## Build steps
You need `Docker for Desktop` installed in your machine for MacOS or Windows. 
You also need `VSCode` with `Dev Containers` extensions.

Open the project with VSCode, you should see the `Reopen in Container` button from VSCode. Click the button and wait until container set up ends.

Once done, open a terminal in vscode and type

``` bash
./fake.sh
```

Then navigate to 

```
http://localhost:8000
```

### Sending verification emails
This step is optional. Sign in requires send grid key, to send verification code. If you don't have it, you can find the verification code from the logs in terminal. If you have it, you need to provide via asp.net config. I use a file called secrets.hocon, must be inside debug/bin folder of Server project when running with content:

```hocon
config{
    SendGrid.APIKEY = "<your_key>"
}
```

## To build for production 

``` bash
./fake.sh PublishServer
```
Then grap the `deploy` folder
