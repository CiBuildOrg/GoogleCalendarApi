# .NET Core (.net standard 2.0) Starter

## Disclaimer

This is currently under development. Not all things might work. The stable versions will be included under releases (when any). 

This will serve as a general base for creating a backend project in .NET core 2.0 with [OpenIdDict](https://github.com/openiddict) authentication

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

Create a database called ```MvcServer``` and start the server. You can get started with the postman collection from ```/docs/postman```

In case you want to change the database name, you can do that in ```/src/MvcServer/config.json```

### Prerequisites

You need Visual Studio 2017 to compile this and .net core 2.0. ```NuGet``` will take care of the rest.

### Installing

Pull the repo, open it in Visual Studio and enable NuGet package restore. I assume you gave your database the name ```MvcServer```. If this is not the case, please change it accordingly in ```config.json```

Start the project

## Deployment

More to come. Will contain a build script capable of deploy using [Cake](http://cakebuild.net/).

## Contributing

Please read [CONTRIBUTING.md](https://github.com/CiBuildOrg/Core-Web-Api/blob/master/CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/CiBuildOrg/Core-Web-Api/tags). 


## License

This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/CiBuildOrg/Core-Web-Api/blob/master/LICENSE) file for details
