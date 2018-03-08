var gulp = require('gulp');
var clean = require('gulp-clean');
var zip = require('gulp-zip');
var merge = require('merge-stream');

gulp.task('clean', function () {
    var build = gulp.src('ts/out', { read: false })
        .pipe(clean());
    var dist = gulp.src('editor.zip', { read: false })
        .pipe(clean());

    return merge(build, dist);
});

gulp.task('build',['clean'], function () {
    var editorHtml = gulp.src('ts/editor.html')
        .pipe(gulp.dest('ts/out'));

    var editorJs = gulp.src('ts/editor.js')
        .pipe(gulp.dest('ts/out'));

    var diffHtml = gulp.src('ts/diff.html')
        .pipe(gulp.dest('ts/out'));

    var diffJs = gulp.src('ts/diff.js')
        .pipe(gulp.dest('ts/out'));

    var monaco = gulp.src('node_modules/monaco-editor/min/vs/**')
        .pipe(gulp.dest('ts/out/vs'));

    return merge(editorHtml, editorJs, diffHtml, diffJs, monaco);
});

gulp.task('zip', ['build'], function () {
    return gulp.src('ts/out/**')
        .pipe(zip('editor.zip'))
        .pipe(gulp.dest(''));
});

gulp.task('default', ['zip']);