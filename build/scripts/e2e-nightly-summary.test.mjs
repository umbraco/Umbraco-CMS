// Run with: node --test build/scripts/e2e-nightly-summary.test.mjs
import assert from 'node:assert/strict';
import {mkdir, mkdtemp, rm, writeFile} from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import {test} from 'node:test';
import {aggregate, buildSummary, findFailedStages, findMissingJobs, flattenReport, loadJobs, loadPrevious, renderMarkdown, renderSlack} from './e2e-nightly-summary.mjs';

const build = {id: '2', number: '19.1.0-test.2', branch: 'v19/dev', commit: 'abc', reason: 'Schedule', url: 'https://example.test/build?buildId=2'};

function report(file, outcomes, error) {
  return {
    errors: [],
    suites: [{
      title: file,
      file,
      specs: [],
      suites: [{
        title: 'Area',
        file,
        specs: Object.entries(outcomes).map(([title, status]) => ({
          title,
          file,
          tests: [{
            status,
            results: status === 'expected' || status === 'skipped' ? [{status: 'passed'}] : [{status: 'failed', error: {message: `\u001b[31m${error ?? `Error: ${title} broke`}\u001b[39m\n  at somewhere`}}],
          }],
        })),
      }],
    }],
  };
}

function job(name, os, outcomes, stage = 'DefaultConfigE2E') {
  return {stage, job: name, os, attempt: 1, report: outcomes ? report('DefaultConfig/Content/Content.spec.ts', outcomes) : null};
}

function summarize(jobs, previous, failedStages) {
  const {tests, infra} = aggregate(jobs);
  return buildSummary({tests, infra, previous, build, failedStages});
}

function slackText(message) {
  return [...message.blocks.map(block => block.text?.text ?? ''), ...message.attachments.map(attachment => attachment.text)].join('\n');
}

const key = title => `DefaultConfig/Content/Content.spec.ts › Area › ${title}`;

test('a test is identified by file, describe blocks and title, with a clean first error line', () => {
  const [entry] = [...flattenReport(report('DefaultConfig\\Content\\Content.spec.ts', {'can save': 'unexpected'}))];
  assert.equal(entry.key, key('can save'));
  assert.equal(entry.outcome, 'failed');
  assert.equal(entry.error, 'Error: can save broke');
});

test('a test counts once, by its worst outcome across jobs, listing where it failed', () => {
  const summary = summarize([
    job('E2E Linux', 'Linux', {'can save': 'unexpected', 'can publish': 'expected', 'can sort': 'flaky', 'can copy': 'skipped'}),
    job('E2E Windows', 'Windows_NT', {'can save': 'unexpected', 'can publish': 'flaky', 'can sort': 'expected', 'can copy': 'skipped'}),
  ]);
  assert.deepEqual(summary.counts, {failed: 1, flaky: 2, passed: 0, skipped: 1});
  assert.deepEqual(summary.newFailures.map(entry => [entry.key, entry.jobs]), [[key('can save'), ['Linux', 'Windows']]]);
  assert.deepEqual(summary.flaky.find(entry => entry.key === key('can publish')).jobs, ['Windows']);
});

test('failures are split into new, still failing and passing again against the previous run', () => {
  const previous = {
    build: {id: '1'},
    failing: {
      [key('can save')]: {runs: 3, since: '19.1.0-test.0'},
      [key('can publish')]: {runs: 2, since: '19.1.0-test.1'},
      [key('can copy')]: {runs: 1, since: '19.1.0-test.1'},
    },
  };
  const summary = summarize([job('E2E Linux', 'Linux', {'can save': 'unexpected', 'can publish': 'expected', 'can copy': 'expected', 'can sort': 'unexpected'})], previous);
  assert.deepEqual(summary.newFailures.map(entry => entry.key), [key('can sort')]);
  assert.deepEqual(summary.stillFailing.map(entry => [entry.key, entry.runs]), [[key('can save'), 4]]);
  assert.deepEqual(summary.passingAgain, [key('can publish')]);
  assert.deepEqual(summary.failing[key('can save')], {runs: 4, since: '19.1.0-test.0', jobs: ['Linux']});
  assert.equal(summary.failing[key('can publish')], undefined);
  assert.equal(summary.failing[key('can copy')], undefined);
});

test('a known failure stays open until a complete run shows it passing', () => {
  const previous = {build: {id: '1'}, failing: {[key('can save')]: {runs: 2, since: 'x'}}};

  const brokenJob = summarize([job('E2E Linux', 'Linux', null)], previous);
  assert.deepEqual(brokenJob.failing[key('can save')], {runs: 2, since: 'x', absent: 1});
  assert.deepEqual(brokenJob.passingAgain, []);

  const passedElsewhere = summarize([job('E2E Linux', 'Linux', {'can save': 'expected'}), job('E2E Windows', 'Windows_NT', null)], previous);
  assert.deepEqual(passedElsewhere.failing[key('can save')], {runs: 2, since: 'x', absent: 1});
  assert.deepEqual(passedElsewhere.passingAgain, []);

  const otherConfigurationOnly = summarize([{...job('E2E Linux', 'Linux', null), report: report('DeliveryApi/Content.spec.ts', {'returns content': 'expected'})}], previous);
  assert.deepEqual(otherConfigurationOnly.failing[key('can save')], {runs: 2, since: 'x', absent: 1});
});

test('a failure seen in one configuration stays open until that configuration passes', () => {
  const sqlite = outcomes => ({...job('E2E SQLite', 'Linux', outcomes), database: 'SQLite'});
  const sqlServer = outcomes => job('E2E Linux', 'Linux', outcomes);

  let summary = summarize([sqlServer({'can save': 'expected'}), sqlite({'can save': 'unexpected'})]);
  summary = summarize([sqlServer({'can save': 'expected'}), sqlite({'can save': 'unexpected'})], summary);
  assert.deepEqual(summary.failing[key('can save')].jobs, ['Linux SQLite']);

  const withoutSqlite = summarize([sqlServer({'can save': 'expected'})], summary);
  assert.deepEqual(withoutSqlite.passingAgain, []);
  assert.equal(withoutSqlite.failing[key('can save')].runs, 2);

  const sqlitePasses = summarize([sqlServer({'can save': 'expected'}), sqlite({'can save': 'expected'})], withoutSqlite);
  assert.deepEqual(sqlitePasses.passingAgain, [key('can save')]);
  assert.deepEqual(sqlitePasses.failing, {});
});

test('a known failure is forgotten when it is skipped, removed from its spec file, or absent for too long', () => {
  const previous = {build: {id: '1'}, failing: {[key('can save')]: {runs: 2, since: 'x', absent: 7}}};

  const skipped = summarize([job('E2E Linux', 'Linux', {'can save': 'skipped'})], previous);
  assert.deepEqual(skipped.failing, {});
  assert.deepEqual(skipped.passingAgain, []);
  assert.deepEqual(skipped.skippedWhileFailing, [key('can save')]);
  assert.match(slackText(renderSlack(skipped)), /Skipped while failing \(1\)/);

  const absentTooLong = summarize([{...job('E2E Linux', 'Linux', null), report: report('DeliveryApi/Content.spec.ts', {'returns content': 'expected'})}], previous);
  assert.deepEqual(absentTooLong.failing, {});

  const removed = summarize([job('E2E Linux', 'Linux', {'can publish': 'expected'})], previous);
  assert.deepEqual(removed.failing, {});
  assert.deepEqual(removed.passingAgain, []);
});

test('jobs that ran but published nothing are reported, skipped jobs are not', () => {
  const timeline = {
    records: [
      {id: 's', type: 'Stage', identifier: 'DefaultConfigE2E'},
      {id: 'p', type: 'Phase', parentId: 's'},
      {id: 'j1', type: 'Job', parentId: 'p', name: 'E2E Linux', result: 'succeeded'},
      {id: 'j2', type: 'Job', parentId: 'p', name: 'E2E Windows', result: 'canceled'},
      {id: 'j3', type: 'Job', parentId: 'p', name: 'E2E SQLite', result: 'skipped'},
      {id: 'o', type: 'Stage', identifier: 'Build'},
      {id: 'op', type: 'Phase', parentId: 'o'},
      {id: 'oj', type: 'Job', parentId: 'op', name: 'Build Umbraco CMS', result: 'succeeded'},
    ],
  };
  const jobs = [job('E2E Linux', 'Linux', {'can save': 'expected'})];
  assert.deepEqual(findMissingJobs(timeline, jobs), [{job: 'E2E Windows', reason: 'job canceled before publishing results'}]);
  const byId = [{...jobs[0], job: 'LinuxPart1Of3', jobId: 'j1'}];
  assert.deepEqual(findMissingJobs(timeline, byId), [{job: 'E2E Windows', reason: 'job canceled before publishing results'}]);
  assert.deepEqual(findMissingJobs(null, jobs), []);
});

test('the Slack message names the branch, caps long lists and escapes markup', () => {
  const outcomes = Object.fromEntries(Array.from({length: 12}, (_, index) => [`case <${index}>`, 'unexpected']));
  const message = renderSlack(summarize([job('E2E Linux', 'Linux', outcomes)], {build: {id: '1'}, failing: {}}));
  const text = slackText(message);
  assert.match(text, /🔴 \*Nightly E2E · v19\/dev\* · <https:\/\/example\.test\/build\?buildId=2\|19\.1\.0-test\.2>/);
  assert.match(text, /\*12 failed\*/);
  assert.match(text, /New since the previous run \(12\)/);
  assert.match(text, /_\+2 more_/);
  assert.match(text, /case &lt;0&gt;/);
  assert.ok(message.attachments.every(attachment => attachment.text.length <= 3000));
});

test('new failures sharing an error line are folded into one Slack entry, others stay separate', () => {
  const shared = Object.fromEntries(['a', 'b', 'c', 'd'].map(title => [`can ${title}`, 'unexpected']));
  const jobs = [
    {...job('E2E Linux', 'Linux', null), report: report('DefaultConfig/Content/Content.spec.ts', shared, 'Error: server did not start')},
    {...job('E2E Windows', 'Windows_NT', null), report: report('DefaultConfig/Media/Media.spec.ts', {'can upload': 'unexpected'})},
  ];
  const text = slackText(renderSlack(summarize(jobs, {build: {id: '1'}, failing: {}})));
  assert.match(text, /New since the previous run \(5\)/);
  assert.match(text, /\*4 tests with the same error\* {2}`Linux`\n {6}↳ Error: server did not start/);
  assert.equal(text.match(/server did not start/g).length, 1);
  assert.match(text, /◦ _\+1 more_/);
  assert.match(text, /• DefaultConfig\/Media\/Media\.spec\.ts › Area › can upload {2}`Windows`\n {6}↳ Error: can upload broke/);
});

test('a failed stage outside the E2E stages is listed without deciding the E2E result', () => {
  const timeline = {
    records: [
      {id: 'b', type: 'Stage', identifier: 'Build', name: 'Build', result: 'succeeded'},
      {id: 'i', type: 'Stage', identifier: 'Integration', name: 'Integration Tests', result: 'failed'},
      {id: 'e', type: 'Stage', identifier: 'DefaultConfigE2E', name: 'Default Config E2E Tests', result: 'failed'},
      {id: 'r', type: 'Stage', identifier: 'Report', name: 'Report E2E Results', result: null},
      {id: 'j', type: 'Job', name: 'Integration Tests', result: 'failed'},
    ],
  };
  const failedStages = findFailedStages(timeline);
  assert.deepEqual(failedStages, [{stage: 'Integration Tests', result: 'failed'}]);
  assert.deepEqual(findFailedStages(null), []);

  const message = renderSlack(summarize([job('E2E Linux', 'Linux', {'can save': 'expected'})], undefined, failedStages));
  assert.match(slackText(message), /^🟢[\s\S]*Other failed stages \(1\)\*\n• Integration Tests — failed/);
  assert.match(renderMarkdown(summarize([job('E2E Linux', 'Linux', {'can save': 'expected'})], undefined, failedStages)), /- Integration Tests: failed/);
});

test('a clean run is green and a run without any results is red', () => {
  const clean = summarize([job('E2E Linux', 'Linux', {'can save': 'expected'})]);
  assert.match(renderSlack(clean).blocks[0].text.text, /^🟢/);
  assert.match(renderMarkdown(clean), /0 failed · 0 flaky · 1 passed · 0 skipped/);

  const empty = summarize([]);
  assert.match(renderSlack(empty).blocks[0].text.text, /^🔴[\s\S]*No E2E results were produced/);
});

test('a job summary is superseded by a later attempt, not by another job with the same name', async () => {
  const dir = await mkdtemp(path.join(os.tmpdir(), 'e2e-summary-'));
  try {
    const publish = async (folder, meta, outcomes) => {
      await mkdir(path.join(dir, folder));
      await writeFile(path.join(dir, folder, 'job.json'), JSON.stringify({stage: 'DefaultConfigE2E', job: 'LinuxPart1Of3', os: 'Linux', ...meta}));
      await writeFile(path.join(dir, folder, 'results.json'), JSON.stringify(report('DefaultConfig/Content/Content.spec.ts', outcomes)));
    };
    await publish('sqlite', {jobId: 'a', attempt: '2', database: 'SQLite'}, {'can save': 'unexpected'});
    await publish('sqlserver-first', {jobId: 'b', attempt: '1', database: 'SQLServer'}, {'can save': 'unexpected'});
    await publish('sqlserver-rerun', {jobId: 'c', attempt: '2', database: 'SQLServer'}, {'can save': 'expected'});
    await mkdir(path.join(dir, 'unreadable'));
    await writeFile(path.join(dir, 'unreadable', 'job.json'), '{');

    const jobs = await loadJobs(dir);
    assert.deepEqual(jobs.map(entry => entry.jobId).sort(), ['a', 'c']);
    const summary = buildSummary({...aggregate(jobs), build});
    assert.deepEqual(summary.newFailures.map(entry => entry.jobs), [['Linux SQLite']]);
  } finally {
    await rm(dir, {recursive: true, force: true});
  }
});

test('a job that was not rerun keeps its summary when a job with the same name was', async () => {
  const dir = await mkdtemp(path.join(os.tmpdir(), 'e2e-summary-'));
  try {
    const publish = async (folder, meta) => {
      await mkdir(path.join(dir, folder));
      await writeFile(path.join(dir, folder, 'job.json'), JSON.stringify({stage: 'DefaultConfigE2E', job: 'LinuxPart1Of3', os: 'Linux', ...meta}));
    };
    await publish('sqlite', {jobId: 'a', attempt: '1', database: 'SQLite'});
    await publish('sqlserver-first', {jobId: 'b', attempt: '1', database: 'SQLServer'});
    await publish('sqlserver-rerun', {jobId: 'c', attempt: '2', database: 'SQLServer'});

    assert.deepEqual((await loadJobs(dir)).map(entry => entry.jobId).sort(), ['a', 'c']);
  } finally {
    await rm(dir, {recursive: true, force: true});
  }
});

test('the previous run is read from its latest attempt', async () => {
  const dir = await mkdtemp(path.join(os.tmpdir(), 'e2e-previous-'));
  try {
    for (const attempt of [2, 3, 1]) {
      const folder = path.join(dir, `e2e-nightly-summary - Attempt #${attempt}`);
      await mkdir(folder);
      await writeFile(path.join(folder, 'summary.json'), JSON.stringify({build: {id: '1', attempt}, failing: {}}));
    }
    await mkdir(path.join(dir, 'unreadable'));
    await writeFile(path.join(dir, 'unreadable', 'summary.json'), '{');

    assert.equal((await loadPrevious(dir, build)).build.attempt, 3);
    assert.equal(await loadPrevious(dir, {...build, id: '1'}), null);
    assert.equal(await loadPrevious(path.join(dir, 'missing'), build), null);
  } finally {
    await rm(dir, {recursive: true, force: true});
  }
});

test('the error line names the element a failure was waiting on', () => {
  const failing = (error, errors) => ({
    suites: [{title: 'a.spec.ts', file: 'a.spec.ts', specs: [{title: 'case', file: 'a.spec.ts', tests: [{status: 'unexpected', results: [{error, errors}]}]}]}],
  });
  const describe = (error, errors) => [...flattenReport(failing(error, errors))][0].error;

  assert.equal(
    describe({message: "Error: expect(locator).toBeVisible() failed\n\nLocator: locator('umb-tree')\nExpected: visible"}),
    "Error: expect(locator).toBeVisible() failed · locator('umb-tree')");
  assert.equal(
    describe({message: "TimeoutError: locator.click: Timeout 300ms exceeded.\nCall log:\n  - waiting for getByTestId('save')\n"}),
    "TimeoutError: locator.click: Timeout 300ms exceeded. · getByTestId('save')");
  assert.equal(
    describe(
      {message: 'Test timeout of 60000ms exceeded.'},
      [{message: 'Test timeout of 60000ms exceeded.'}, {message: "Error: locator.click: Test timeout of 60000ms exceeded.\nCall log:\n  - waiting for locator('umb-modal')"}]),
    "Test timeout of 60000ms exceeded. · locator('umb-modal')");
  assert.equal(describe({message: 'Error: expect(received).toBe(expected)\n\nExpected: 1'}), 'Error: expect(received).toBe(expected)');
});

test('a test that keeps coming back is marked, in new failures and in flaky tests', () => {
  const runs = [
    {'can save': 'unexpected', 'can sort': 'flaky'},
    {'can save': 'expected', 'can sort': 'flaky'},
    {'can save': 'unexpected', 'can sort': 'expected'},
    {'can save': 'expected', 'can sort': 'flaky'},
    {'can save': 'unexpected', 'can sort': 'flaky'},
  ];
  let summary;
  for (const outcomes of runs) {
    summary = summarize([job('E2E Linux', 'Linux', outcomes)], summary);
  }

  assert.deepEqual(summary.newFailures.map(entry => [entry.key, entry.earlierRuns]), [[key('can save'), 2]]);
  assert.deepEqual(summary.flaky.map(entry => [entry.key, entry.runs]), [[key('can sort'), 4]]);
  const text = renderSlack(summary).attachments.map(attachment => attachment.text).join('\n');
  assert.match(text, /can save.*also failed in 2 earlier runs/);
  assert.match(text, /can sort.*flaky in 4 recent runs/);
  assert.match(renderMarkdown(summary), /also failed in 2 earlier runs/);
});

test('a test is forgotten once it has neither failed nor been flaky for a while', () => {
  let summary = summarize([job('E2E Linux', 'Linux', {'can save': 'unexpected'})]);
  for (let run = 0; run < 14; run++) {
    summary = summarize([job('E2E Linux', 'Linux', {'can save': 'expected'})], summary);
  }
  assert.deepEqual(summary.history.failed[key('can save')], {count: 1, age: 14});

  summary = summarize([job('E2E Linux', 'Linux', {'can save': 'expected'})], summary);
  assert.deepEqual(summary.history.failed, {});
});

test('a rerun is marked in the Slack header', () => {
  const summary = buildSummary({...aggregate([job('E2E Linux', 'Linux', {'can save': 'expected'})]), build: {...build, attempt: 2}});
  assert.match(renderSlack(summary).blocks[0].text.text, /· attempt 2$/m);
  assert.doesNotMatch(renderSlack(summarize([job('E2E Linux', 'Linux', {'can save': 'expected'})])).blocks[0].text.text, /attempt/);
});
