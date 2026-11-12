// Run with: node --test build/scripts/e2e-nightly-summary.test.mjs
import assert from 'node:assert/strict';
import {test} from 'node:test';
import {aggregate, buildSummary, findMissingJobs, flattenReport, renderMarkdown, renderSlack} from './e2e-nightly-summary.mjs';

const build = {id: '2', number: '19.1.0-test.2', branch: 'v19/dev', commit: 'abc', reason: 'Schedule', url: 'https://example.test/build?buildId=2'};

function report(file, outcomes) {
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
            results: status === 'expected' || status === 'skipped' ? [{status: 'passed'}] : [{status: 'failed', error: {message: `\u001b[31mError: ${title} broke\u001b[39m\n  at somewhere`}}],
          }],
        })),
      }],
    }],
  };
}

function job(name, os, outcomes, stage = 'DefaultConfigE2E') {
  return {stage, job: name, os, attempt: 1, report: outcomes ? report('DefaultConfig/Content/Content.spec.ts', outcomes) : null};
}

function summarize(jobs, previous) {
  const {tests, infra} = aggregate(jobs);
  return buildSummary({tests, infra, previous, build});
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

test('failures are split into new, still failing and fixed against the previous run', () => {
  const previous = {
    build: {id: '1'},
    failing: {
      [key('can save')]: {runs: 3, since: '19.1.0-test.0'},
      [key('can publish')]: {runs: 1, since: '19.1.0-test.1'},
    },
  };
  const summary = summarize([job('E2E Linux', 'Linux', {'can save': 'unexpected', 'can publish': 'expected', 'can sort': 'unexpected'})], previous);
  assert.deepEqual(summary.newFailures.map(entry => entry.key), [key('can sort')]);
  assert.deepEqual(summary.stillFailing.map(entry => [entry.key, entry.runs]), [[key('can save'), 4]]);
  assert.deepEqual(summary.fixed, [key('can publish')]);
  assert.deepEqual(summary.failing[key('can save')], {runs: 4, since: '19.1.0-test.0'});
  assert.equal(summary.failing[key('can publish')], undefined);
});

test('a known failure stays open until a complete run shows it passing', () => {
  const previous = {build: {id: '1'}, failing: {[key('can save')]: {runs: 2, since: 'x'}}};

  const brokenJob = summarize([job('E2E Linux', 'Linux', null)], previous);
  assert.deepEqual(brokenJob.failing[key('can save')], {runs: 2, since: 'x', absent: 1});
  assert.deepEqual(brokenJob.fixed, []);

  const passedElsewhere = summarize([job('E2E Linux', 'Linux', {'can save': 'expected'}), job('E2E Windows', 'Windows_NT', null)], previous);
  assert.deepEqual(passedElsewhere.failing[key('can save')], {runs: 2, since: 'x'});
  assert.deepEqual(passedElsewhere.fixed, []);

  const otherConfigurationOnly = summarize([{...job('E2E Linux', 'Linux', null), report: report('DeliveryApi/Content.spec.ts', {'returns content': 'expected'})}], previous);
  assert.deepEqual(otherConfigurationOnly.failing[key('can save')], {runs: 2, since: 'x', absent: 1});
});

test('a known failure is forgotten when it is skipped, removed from its spec file, or absent for too long', () => {
  const previous = {build: {id: '1'}, failing: {[key('can save')]: {runs: 2, since: 'x', absent: 7}}};

  const skipped = summarize([job('E2E Linux', 'Linux', {'can save': 'skipped'})], previous);
  assert.deepEqual(skipped.failing, {});

  const absentTooLong = summarize([{...job('E2E Linux', 'Linux', null), report: report('DeliveryApi/Content.spec.ts', {'returns content': 'expected'})}], previous);
  assert.deepEqual(absentTooLong.failing, {});

  const removed = summarize([job('E2E Linux', 'Linux', {'can publish': 'expected'})], previous);
  assert.deepEqual(removed.failing, {});
  assert.deepEqual(removed.fixed, []);
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
  const text = message.blocks.map(block => block.text?.text ?? '').join('\n');
  assert.match(text, /🔴 \*Nightly E2E · v19\/dev\*/);
  assert.match(text, /\*12 failed\*/);
  assert.match(text, /New since the previous run \(12\)/);
  assert.match(text, /_\+2 more_/);
  assert.match(text, /case &lt;0&gt;/);
  assert.ok(message.blocks.every(block => !block.text || block.text.text.length <= 3000));
});

test('a clean run is green and a run without any results is red', () => {
  const clean = summarize([job('E2E Linux', 'Linux', {'can save': 'expected'})]);
  assert.match(renderSlack(clean).blocks[0].text.text, /^🟢/);
  assert.match(renderMarkdown(clean), /0 failed · 0 flaky · 1 passed · 0 skipped/);

  const empty = summarize([]);
  assert.match(renderSlack(empty).blocks[0].text.text, /^🔴[\s\S]*No E2E results were produced/);
});
