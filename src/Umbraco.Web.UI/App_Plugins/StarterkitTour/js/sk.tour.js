//If a user has finished the tour before or decided to skip it then 
//We won't show it again
if(localStorage.getItem('tour') != "completed"){

    //initialize instance
    var enjoyhint_instance = new EnjoyHint({
    onStart:function(){

    },
    onNext: function(){
        
    },
    onSkip: function(){
        localStorage.setItem('tour', "completed");
    },
    onEnd: function(){
        localStorage.setItem('tour', "completed");
    }
    });

    //Tour configuration/steps
    var enjoyhint_script_steps = [
        {
            selector: '.sections',
            event_type:'next',
            description: '<h3>Welcome to Umbraco CMS</h3>This area is defined as <em>sections</em> and allows you to navigate the different areas of Umbraco',
            margin: 0,
            right:2,
            nextButton : {
                className: 'myNextClass', 
                text: 'Next'
            },
            showSkip: false
        },
        {
            selector: '#tree',
            event_type:'next',
            description: 'This section is referred to the tree and this contains the pages of you website',
            margin: 0,
            nextButton : {
                className: 'myNextClass', 
                text: 'Next'
            },
            showSkip: false
        },
        {
            selector:'.form-search',
            event_type:'next',
            description: 'Search allows you to quickly find content within your site',
            margin: 0,
            nextButton : {
                className: 'myNextClass', 
                text: 'Next'
            },
            showSkip: false
        },
        {
            selector:'.umb-avatar',
            event_selector:'#applications .sections li.avatar',
            event:'click',
            description: 'By clicking here you can edit your profile and change your password',
            shape: 'circle',
            radius: 30,
            showSkip: false
        },
        {
            selector:'button localize[key=general_changePassword]',
            event_type:'next',
            description:'This is where you can quickly & easily find where to change your password at a later date',
            margin:0,
            left:-14,
            top:-6,
            bottom:-5,
            right:-13,
            showSkip: false
        },
        {
            selector: 'li.help',
            event: 'click',
            description: 'Click here to end the tour & carry on learning Umbraco',
            margin:0,
            right:0,
            showSkip: false
        }
    ];

    //set script config
    enjoyhint_instance.set(enjoyhint_script_steps);

    //run Enjoyhint script
    enjoyhint_instance.run();
}