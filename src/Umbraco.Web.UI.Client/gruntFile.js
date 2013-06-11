module.exports = function (grunt) {

  grunt.loadNpmTasks('grunt-contrib-concat');
  grunt.loadNpmTasks('grunt-contrib-jshint');
  grunt.loadNpmTasks('grunt-contrib-uglify');
  grunt.loadNpmTasks('grunt-contrib-clean');
  grunt.loadNpmTasks('grunt-contrib-copy');
  grunt.loadNpmTasks('grunt-contrib-watch');
  grunt.loadNpmTasks('grunt-recess');
  grunt.loadNpmTasks('grunt-testacular');
  grunt.loadNpmTasks('grunt-open');
  grunt.loadNpmTasks('grunt-markdown');
  grunt.loadNpmTasks('grunt-contrib-connect');

  // Default task.
  grunt.registerTask('default', ['jshint:dev','build','testacular:unit']);
  grunt.registerTask('dev', ['jshint:dev', 'build', 'webserver', 'open:dev', 'watch']);

  //run by the watch task
  grunt.registerTask('watch-build', ['jshint:dev','recess:build','testacular:unit','concat','copy']);
  
  //triggered from grunt dev or grunt
  grunt.registerTask('build', ['clean','concat','recess:build','copy']);

  //utillity tasks
  grunt.registerTask('docs', ['markdown']);
  grunt.registerTask('webserver', ['connect:devserver']);

  // Print a timestamp (useful for when watching)
  grunt.registerTask('timestamp', function() {
    grunt.log.subhead(Date());
  });

  var testacularConfig = function(configFile, customOptions) {
    var options = { configFile: configFile, keepalive: true };
    var travisOptions = process.env.TRAVIS && { browsers: ['Firefox'], reporters: 'dots' };
    return grunt.util._.extend(options, customOptions, travisOptions);
  };

  // Project configuration.
  grunt.initConfig({
    connect: {
             devserver: {
               options: {
                 port: 8080,
                 hostname: '0.0.0.0',
                 base: './build',
                 middleware: function(connect, options){
                   return [
                     //uncomment to enable CSP
                     // util.csp(),
                     //util.rewrite(),
                     connect.favicon('images/favicon.ico'),
                     connect.static(options.base),
                     connect.directory(options.base)
                   ];
                 }
               }
             },
             testserver: {}
           },

    open : {
      dev : {
        path: 'http://localhost:8080/belle/'
      }
    },

    distdir: 'build/belle',
    pkg: grunt.file.readJSON('package.json'),
    banner:
    '/*! <%= pkg.title || pkg.name %> - v<%= pkg.version %> - <%= grunt.template.today("yyyy-mm-dd") %>\n' +
    '<%= pkg.homepage ? " * " + pkg.homepage + "\\n" : "" %>' +
    ' * Copyright (c) <%= grunt.template.today("yyyy") %> <%= pkg.author %>;\n' +
    ' * Licensed <%= _.pluck(pkg.licenses, "type").join(", ") %>\n */\n',
    src: {
      js: ['src/**/*.js', '<%= distdir %>/templates/**/*.js'],
      common: ['src/common/**/*.js'],
      specs: ['test/**/*.spec.js'],
      scenarios: ['test/**/*.scenario.js'],
      samples: ['sample files/*.js'],
      html: ['src/index.html'],

      everything:['src/**/*.*', 'test/**/*.*', 'docs/**/*.*'],

      tpl: {
        app: ['src/views/**/*.html'],
        common: ['src/common/**/*.tpl.html']
      },
      less: ['src/less/belle.less'], // recess:build doesn't accept ** in its file patterns
      prod: ['<%= distdir %>/js/*.js']
    },

    clean: ['<%= distdir %>/*'],

    copy: {
      assets: {
        files: [{ dest: '<%= distdir %>/assets', src : '**', expand: true, cwd: 'src/assets/' }]
      },
      vendor: {
        files: [{ dest: '<%= distdir %>/lib', src : '**', expand: true, cwd: 'lib/' }]
      },
      views: {
        files: [{ dest: '<%= distdir %>/views', src : '**/*.*', expand: true, cwd: 'src/views/' }]
      },
      app: {
        files: [
            { dest: '<%= distdir %>/js', src : 'main.js', expand: true, cwd: 'src/' },
            { dest: '<%= distdir %>/js', src : 'routes.js', expand: true, cwd: 'src/' }]
      },
      media: {
        files: [{ dest: 'build/media', src : '*.*', expand: true, cwd: 'media/' }]
      },
      sampleFiles: {
        files: [{ dest: '<%= distdir %>/js', src : '*.js', expand: true, cwd: 'src/sample files/' }]
      }
    },

    testacular: {
      unit: { options: testacularConfig('test/config/unit.js') },
      e2e: { options: testacularConfig('test/config/e2e.js') },
      watch: { options: testacularConfig('test/config/unit.js', {singleRun:false, autoWatch: true}) }
    },

    concat:{
      index: {
        src: ['src/index.html'],
        dest: '<%= distdir %>/index.html',
        options: {
          process: true
        }
      },
      app: {
        src: ['src/app.js'],
        dest: '<%= distdir %>/js/app.js',
        options:{
          banner: "<%= banner %>'use strict';\ndefine(['angular'], function (angular) {\n",
          footer: "\n\nreturn app;\n});"
        }
      },
      angular: {
        src:['vendor/angular/angular.min.js'],
        dest: '<%= distdir %>/lib/angular/angular.min.js'
      },
      controllers: {
        src:['src/views/**/*.controller.js'],
        dest: '<%= distdir %>/js/umbraco.controllers.js',
        options:{
          banner: "'use strict';\n<%= banner %>\ndefine(['app', 'angular'], function (app, angular) {\n",
          footer: "\n\nreturn angular;\n});"
        }
      },
      services: {
        src:['src/common/services/*.js'],
        dest: '<%= distdir %>/js/umbraco.services.js',
        options:{
          banner: "'use strict';\ndefine(['app','angular'], function (app, angular) {\n",
          footer: "\n\nreturn angular;\n});"
        }
      },
      resources: {
        src:['src/common/resources/*.js'],
        dest: '<%= distdir %>/js/umbraco.resources.js',
        options:{
          banner: "<%= banner %>'use strict';\ndefine(['app', 'angular'], function (app, angular) {\n",
          footer: "\n\nreturn angular;\n});"
        }
      },
      mocks: {
        src:['src/common/mocks/**/*.js'],
        dest: '<%= distdir %>/js/umbraco.mocks.js',
        options:{
          banner: "<%= banner %>'use strict';\ndefine(['app', 'angular'], function (app, angular) {\n",
          footer: "\n\nreturn angular;\n});"
        }
      },
      directives: {
        src:['src/common/directives/*.js'],
        dest: '<%= distdir %>/js/umbraco.directives.js',
        options:{
          banner: "<%= banner %>'use strict';\ndefine(['app','angular','underscore'], function (app, angular,_) {\n",
          footer: "\n\nreturn angular;\n});"
        }
      },
      filters: {
        src:['src/common/filters/*.js'],
        dest: '<%= distdir %>/js/umbraco.filters.js',
        options:{
          banner: "<%= banner %>'use strict';\ndefine([ 'app','angular'], function (app,angular) {\n",
          footer: "\n\nreturn app;\n});"
        }
      }
    },

    uglify: {
      dist:{
        options: {
          banner: "<%= banner %>"
        },
        src:['<%= src.js %>'],
        dest:'<%= distdir %>/<%= pkg.name %>.js'
      },
      angular: {
        src:['<%= concat.angular.src %>'],
        dest: '<%= distdir %>/angular.js'
      },
      jquery: {
        src:['vendor/jquery/*.js'],
        dest: '<%= distdir %>/jquery.js'
      }
    },

    recess: {
      build: {
        files: {
          '<%= distdir %>/assets/css/<%= pkg.name %>.css':
          ['<%= src.less %>'] },
        options: {
          compile: true
        }
      },
      min: {
        files: {
          '<%= distdir %>/assets/css/<%= pkg.name %>.css': ['<%= src.less %>']
        },
        options: {
          compress: true
        }
      }
    },

    watch:{
      build: {
        files:['<%= src.everything %>'],
        tasks:['watch-build','timestamp']
      }
    },

    markdown: {
        all: {
          files: ['docs/src/*.md'],
          dest: 'docs/html/'
        }
    },


    jshint:{
      dev:{
         files:['<%= src.common %>', '<%= src.specs %>', '<%= src.scenarios %>', '<%= src.samples %>'],
         options:{
           curly:true,
           eqeqeq:true,
           immed:true,
           latedef:true,
           newcap:true,
           noarg:true,
           sub:true,
           boss:true,
           eqnull:true,
           globals:{}
         } 
      },
      build:{
         files:['<%= src.prod %>'],
         options:{
           curly:true,
           eqeqeq:true,
           immed:true,
           latedef:true,
           newcap:true,
           noarg:true,
           sub:true,
           boss:true,
           eqnull:true,
           globalstrict:true,
           globals:{$:false, jQuery:false,define:false,require:false,window:false}
         } 
      }
    }
  });

};
