const prompt = require('prompt');
fs = require('fs');

const properties = [
    {
        name: 'username'
    },
    {
        name: 'password',
        hidden: true
    }
];

prompt.start();

prompt.get(properties, function (error, result) {
    if (error) { return onError(error); }

    console.log('Saving...');

var fileContent = `{
    "username": "${result.username}",
    "password": "${result.password}"
}`;

    fs.writeFile('cypress.env.json', fileContent, function (error) {
        if (error) return console.error(error);
        console.log('Saved');
      });
});

function onError(error) {
    console.error(error);
    return true;
}
