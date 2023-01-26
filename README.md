# News Aggregator

* [Technologies and tools](#technologies-and-tools)
* [Project description](#project-description)
* [MVC application features](#mvc-application-features)
* [Wep API application features](#web-api-application-features)

## Technologies and tools
**Implementation language:** C#  
**Platform:** .NET 6  
**Database:** Microsoft SQL Server 2019  
  
**Key technologies:**
* ASP.NET Core MVC
* ASP.NET Core Web API
* Entity Framework Core  
  
**Applied patterns and principles:**
* *Dependency Injection*
* *Unit of Work* in combination with *Repository* and *Generic Repository*  
* *Mediator* using an in-process messaging library called *MediatR*  
* *Command Query Separation (CQS)*


## Project description
This application collects articles from news sources and displays articles with a certain rating.
By default, articles with a rating higher than '0' get into the news feed for users.
The administrator can change the rating for the news that gets into the feed.

**News sources used:**
* https://www.onliner.by  
* https://devby.io  
* https://shazoo.ru

**Algorithm for receiving news**  
At the first stage, we receive information from the RSS feed of sources. Then, using the *Html Agility Pack* and an algorithm unique for each source, we get the text of the article. Since we receive html, the main task of the algorithm is to eliminate unnecessary elements, leaving only the text of the article.

**Algorithm for obtaining an article rating**  
Articles are rated using the dictionary-based algorithm AFINN. The algorithm is based on sentiment analysis developed by Finn Arup Nielsen and used by Twitter.
Each word in the dictionary has a rating from -5 to +5. But before we get the rating of the article, we run the text of the article through the lemmatization algorithm to get the lemma of each word from the article. Then we proceed to the evaluation of the article: we assign a score to each lemma from the article, sum the result and divide by the number of words, thus calculating the arithmetic mean. The result of the assessment is recorded in the database.

**User categories**  
* unauthorized user
* user
* administrator


## MVC application features

MVC application uses browser as user interface. The user has the ability to use the application without creating his own profile or by logging into his account. The application provides the user with the opportunity to register. The data is validated: the login must match the email address and be unique (when entering an existing login, the application will notify the user about this), the password must contain at least 8 characters.  
Unauthorized users are only allowed to view preview news. News preview includes title, category name, short description and publication date. An authorized user gets access to view the full text of articles. The administrator has access to additional features, as well as a personal account. 

The administrator can:
1. change the rating of news that will be included in the feed;
2. manually upload articles to the feed, the functionality is available for downloading news from a specific source, as well as randomly from any available sources;
3. create a custom article;
4. make changes to articles in the feed.

## Web API application features
* Web API application uses *Swagger UI* as user interface.  
* The application can receive data from the database about sources, about articles and about users. It is also possible to add a custom article or modify an existing one.  
* The application implements an authentication and authorization mechanism using JWT tokens.
* Recurring Jobs have been created for the algorithms for receiving news, receiving text and receiving ratings. Users have access to Hangfire Dashboard UI to manage jobs.
