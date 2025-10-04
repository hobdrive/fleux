
Fleux
======

Fleux is a simple UI engine that provides a fluid touch friendly experience and is written on .net
It includes an Animation framework, Gestures, and nice controls.

Fleux was originally created by Jose Gallarado at fleux.codeplex.com for Windows Mobile platform.

But the original code was abadoned.

This fork is a continuation of the original project.

The main improvement is that Fleux runs on:

- Windows Mobile, WindowsCE, Windows XP/Vista/7 and Linux seamlessly.
- Android, Windows Phone and iOS (on top of great Xamarin product)
- XNA-based adaptation, giving fleux a real boost in performance. XNA port is abandoned because of Xamarin deprecation and other problems.
- dotnet MAUI + Skiasharp port - the latest generation.

This means you don't have to change a single line in your code to create a truly portable app.

With minimal hooks this allows you to have a single codebase for your app, running on multiple platforms.

The main application of this framework is github.com/cail/hobd project for now.


License
========

Microsoft Public License (Ms-PL)

Credits
========
Jose Gallardo (original author)
cail (this fork)
