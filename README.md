# Envue GraphQL API Gateway

[![CircleCI](https://circleci.com/gh/sea-shanty-2/gateway/tree/master.svg?style=shield)](https://circleci.com/gh/sea-shanty-2/gateway/tree/master)

### Apollo
The API implements a spec-compliant Apollo server which can be queried from any Apollo client. An Apollo server in turn implements a spec-compliant GraphQL server which can be quiered from GraphQL client. An Apollo client can download the API schema from an Apollo server and use it to generate code based on user defined queries.

#### Consuming the API on Android

Download the latest Gradle plugin version of apollo-android and add it to your application:

 <img alt="Download" src="https://api.bintray.com/packages/apollographql/android/apollo-gradle-plugin/images/download.svg"> 


Create a folder called ``graphql`` in the ``src/main/`` directory of your application. Inside the folder, add your namespace folders e.g. ``com/android/example/`` so you end up at the absolute path ``src/main/graphql/com/android/example/``. This folder should hold the API schema ``schema.json`` as well as ``.graphql`` files containing the queries you want to generate code for.


To download the latest version of the Envue GraphQL API Gateway schema run the following command in a terminal:
```
npx apollo schema:download --endpoint=https://envue.me/api <export-path>/schema.json
```

With the schema in place, it's time to add some ``.graphql`` files with queries for the code generator. A recommended way to create the queries, is to fire up a GraphQL client such as [Altair GraphQL Client](https://altair.sirmuel.design/) and write up some test queries to explore the API and figure out what data you need for your application. If you are unsure how to structure queries and mutations, you can check out the [Facebook GraphQL query and mutation documentation](https://graphql.org/learn/queries/). Once you've written out a query you would like to use in your application, copy the query to a ``.graphql`` file.

Here is a ``BroadcastPageQuery`` query for retreiving a page of broadcasts that would go in a file named ``BroadcastPageQuery.graphql`` for reference:
```
query BroadcastPageQuery($after: String, $first: Int) {
  broadcasts {
    page(after: $after, first: $first) {
      items {
        id
        title
      }
    }
  }
}
```

With the plugin installed and the files in place, compiling the code should yield one Java class for each of your queries with nested classes for reading the network response, which you can use to consume the API like so:

```
BroadcastPageQuery broadcastPageQuery = BroadcastPageQuery.builder().after("example_id").first(10).build()
```

For more details, check out the official [Apollo Android Guide](https://www.apollographql.com/docs/android/).
