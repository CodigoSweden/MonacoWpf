var gulp = require('gulp');
var clean = require('gulp-clean');
var zip = require('gulp-zip');
var merge = require('merge-stream');
var typescript = require('gulp-tsc');

gulp.task('clean', function () {
    var build = gulp.src('Site/out', { read: false })
        .pipe(clean());
    var dist = gulp.src('editor.zip', { read: false })
        .pipe(clean());

    return merge(build, dist);
});

gulp.task('compile', ['clean'], function () {
    var editor = gulp.src(['Site/editor.ts', 'Site/monaco.d.ts'])
        .pipe(typescript({ target: "es5", lib: ["es6", "dom"]}))
        .pipe(gulp.dest('Site/'))

    var diff = gulp.src(['Site/diff.ts', 'Site/monaco.d.ts'])
        .pipe(typescript())
        .pipe(gulp.dest('Site/'))

    return merge(editor, diff);
});


gulp.task('build',['compile'], function () {
    var editorHtml = gulp.src('Site/editor.html')
        .pipe(gulp.dest('Site/out'));

    var editorJs = gulp.src('Site/editor.js')
        .pipe(gulp.dest('Site/out'));

    var diffHtml = gulp.src('Site/diff.html')
        .pipe(gulp.dest('Site/out'));

    var diffJs = gulp.src('Site/diff.js')
        .pipe(gulp.dest('Site/out'));

    var es6promise = gulp.src('node_modules/es6-promise/dist/es6-promise.auto.min.js')
        .pipe(gulp.dest('Site/out'));

    var monaco = gulp.src('node_modules/monaco-editor/min/vs/**')
        .pipe(gulp.dest('Site/out/vs'));

    return merge(editorHtml, editorJs, diffHtml, diffJs, es6promise, monaco);
});

gulp.task('zip', ['build'], function () {
    return gulp.src('Site/out/**')
        .pipe(zip('editor.zip'))
        .pipe(gulp.dest(''));
});

gulp.task('default', ['zip']);