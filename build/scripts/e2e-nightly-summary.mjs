#!/usr/bin/env node
// Aggregates the per-job Playwright JSON reports of one pipeline run into a single summary,
// compares it with the previous run's summary and notifies Slack.
//
// Usage: node e2e-nightly-summary.mjs --summaries <dir> --out <dir> [--previous <dir>] [--timeline <file>] [--dry-run]

import {existsSync} from 'node:fs';
import {mkdir, readdir, readFile, writeFile} from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {parseArgs} from 'node:util';

const E2E_STAGES = ['DefaultConfigE2E', 'AdditionalConfigE2E'];
const SLACK_LIST_LIMIT = 10;
const SLACK_SECTION_LIMIT = 2900;
// New failures sharing one error line are shown as a single entry from this many tests on.
const SLACK_GROUP_SIZE = 3;
const SLACK_GROUP_EXAMPLES = 3;
const SLACK_COLORS = {'🔴': '#d72b3f', '🟡': '#e8a317', '🟢': '#2eb67d'};
// A known failure whose spec file stops running altogether (deleted, renamed) is forgotten after this many runs.
const MAX_ABSENT_RUNS = 7;
// A failure seen in a single run that passes in the next is more likely unstable than repaired, so only
// failures that lasted at least this many runs are reported when they pass again.
const MIN_RUNS_TO_REPORT_RECOVERY = 2;
// How many runs a test is remembered after it last failed or was flaky, to recognise one that keeps coming back.
const HISTORY_RUNS = 14;
const OUTCOME_RANK = {skipped: 0, passed: 1, flaky: 2, failed: 3};
const OUTCOME_BY_STATUS = {expected: 'passed', unexpected: 'failed', flaky: 'flaky', skipped: 'skipped'};

async function readJson(file) {
  return JSON.parse((await readFile(file, 'utf8')).replace(/^﻿/, ''));
}

async function findFiles(dir, name) {
  if (!dir || !existsSync(dir)) {
    return [];
  }
  const entries = await readdir(dir, {recursive: true});
  return entries.filter(entry => path.basename(entry) === name).map(entry => path.join(dir, entry));
}

/** Loads every job that published a summary, keeping only the latest attempt of each job. */
export async function loadJobs(dir) {
  const jobs = [];
  for (const metaFile of await findFiles(dir, 'job.json')) {
    let meta;
    try {
      meta = await readJson(metaFile);
    } catch {
      console.warn(`Could not read ${metaFile}; ignoring that job summary.`);
      continue;
    }
    const reportFile = path.join(path.dirname(metaFile), 'results.json');
    let report = null;
    if (existsSync(reportFile)) {
      try {
        report = await readJson(reportFile);
      } catch {
        report = null;
      }
    }
    jobs.push({
      stage: meta.stage,
      job: meta.job,
      displayName: meta.displayName,
      jobId: meta.jobId,
      os: meta.os,
      database: meta.database,
      attempt: Number(meta.attempt) || 1,
      report,
    });
  }

  // Two jobs of one stage can share a name (one matrix run against two databases), so a summary is only
  // superseded by a later attempt of the same job, never by a sibling.
  const name = job => `${job.stage}|${job.job}|${job.database ?? ''}`;
  const latestAttempt = new Map();
  for (const job of jobs) {
    latestAttempt.set(name(job), Math.max(latestAttempt.get(name(job)) ?? 0, job.attempt));
  }
  return jobs.filter(job => job.attempt === latestAttempt.get(name(job)));
}

function clean(text) {
  return String(text ?? '').replace(/\u001b\[[0-9;]*m/g, '');
}

function firstLine(text) {
  return clean(text).split('\n').map(line => line.trim()).find(line => line.length > 0) ?? '';
}

/**
 * Describes a failed attempt in one line. The first line of an assertion or timeout error is the same
 * for every test that fails that way, so the element it was waiting on is added to tell failures apart.
 */
function describeFailure(result) {
  const messages = [result?.error, ...(result?.errors ?? [])].map(error => clean(error?.message));
  const headline = firstLine(messages[0]);
  const target = messages
    .map(message => message.match(/^\s*(?:Locator:|- waiting for)\s*(.+?)\s*$/m)?.[1])
    .find(Boolean);
  return target && !headline.includes(target) ? `${headline} · ${target}` : headline;
}

/** Flattens a Playwright JSON report into one entry per test. */
export function* flattenReport(report) {
  function* walk(suite, titles) {
    for (const spec of suite.specs ?? []) {
      for (const test of spec.tests ?? []) {
        const file = String(spec.file ?? suite.file ?? '').replaceAll('\\', '/');
        const failure = (test.results ?? []).findLast(result => result.error);
        yield {
          key: [file, ...titles, spec.title].join(' › '),
          outcome: OUTCOME_BY_STATUS[test.status] ?? 'failed',
          error: describeFailure(failure),
        };
      }
    }
    for (const child of suite.suites ?? []) {
      yield* walk(child, [...titles, child.title]);
    }
  }

  for (const fileSuite of report.suites ?? []) {
    yield* walk(fileSuite, []);
  }
}

function jobLabel(job) {
  const os = {Windows_NT: 'Windows', Darwin: 'macOS'}[job.os] ?? job.os ?? 'unknown';
  return /sqlite/i.test(`${job.database ?? ''} ${job.job}`) ? `${os} SQLite` : os;
}

/** Merges all jobs into one record per test; a test counts by its worst outcome across jobs. */
export function aggregate(jobs) {
  const tests = new Map();
  const infra = [];

  for (const job of jobs) {
    if (!job.report) {
      infra.push({job: job.displayName || job.job, reason: 'no test results were produced'});
      continue;
    }
    for (const error of job.report.errors ?? []) {
      infra.push({job: job.displayName || job.job, reason: firstLine(error.message) || 'run error'});
    }
    const label = jobLabel(job);
    for (const entry of flattenReport(job.report)) {
      let test = tests.get(entry.key);
      if (!test) {
        test = {key: entry.key, outcome: entry.outcome, failedIn: new Set(), flakyIn: new Set(), error: ''};
        tests.set(entry.key, test);
      }
      if (OUTCOME_RANK[entry.outcome] > OUTCOME_RANK[test.outcome]) {
        test.outcome = entry.outcome;
      }
      if (entry.outcome === 'failed') {
        test.failedIn.add(label);
        test.error ||= entry.error;
      } else if (entry.outcome === 'flaky') {
        test.flakyIn.add(label);
      }
    }
  }

  return {tests, infra};
}

/** Finds E2E jobs that ran according to the build timeline but never published a summary. */
export function findMissingJobs(timeline, jobs) {
  const records = timeline?.records ?? [];
  const stages = new Map(
    records
      .filter(record => record.type === 'Stage')
      .map(record => [record.id, E2E_STAGES.find(stage => [record.identifier, record.refName, record.name].includes(stage))])
      .filter(([, stage]) => stage));
  const phases = new Map(
    records
      .filter(record => record.type === 'Phase' && stages.has(record.parentId))
      .map(record => [record.id, stages.get(record.parentId)]));
  const ran = records.filter(record =>
    record.type === 'Job' && phases.has(record.parentId) && record.result && record.result !== 'skipped');

  const published = new Set(jobs.flatMap(job => [job.jobId, `${job.stage}|${job.job}`]));
  const missing = ran.filter(record =>
    !published.has(record.id) && !published.has(`${phases.get(record.parentId)}|${record.name}`));

  // Jobs that never line up mean the timeline identifies them differently than the agents do;
  // reporting every job as missing would be noise, so trust the published summaries instead.
  if (jobs.length > 0 && ran.length > 0 && missing.length === ran.length) {
    console.warn('Timeline jobs match none of the published summaries; skipping missing-job detection.');
    return [];
  }

  return missing.map(record => ({job: record.name, reason: `job ${record.result} before publishing results`}));
}

/**
 * Finds stages other than the E2E ones that failed, so a clean E2E result is not read as a clean run.
 * They are listed, but the result of the message stays that of the E2E tests it reports on.
 */
export function findFailedStages(timeline) {
  return (timeline?.records ?? [])
    .filter(record => record.type === 'Stage' && ['failed', 'canceled'].includes(record.result))
    .filter(record => !E2E_STAGES.some(stage => [record.identifier, record.refName, record.name].includes(stage)))
    .map(record => ({stage: record.name ?? record.identifier, result: record.result}));
}

function fileOf(key) {
  return key.split(' › ')[0];
}

/** Carries the per-test counters one run forward: tests seen now count once more, the rest age out. */
function updateHistory(previous = {}, keys) {
  const history = {};
  for (const [key, entry] of Object.entries(previous)) {
    if (entry.age < HISTORY_RUNS) {
      history[key] = {count: entry.count, age: entry.age + 1};
    }
  }
  for (const key of keys) {
    history[key] = {count: (previous[key]?.count ?? 0) + 1, age: 0};
  }
  return history;
}

function sortedLabels(set) {
  return [...set].sort();
}

/** Classifies the run against the previous summary and builds the summary document. */
export function buildSummary({tests, infra, previous, build, failedStages = []}) {
  const all = [...tests.values()];
  const count = outcome => all.filter(test => test.outcome === outcome).length;
  const previousFailing = previous?.failing ?? {};
  const failing = {};
  const newFailures = [];
  const stillFailing = [];

  for (const test of all.filter(test => test.outcome === 'failed').sort((a, b) => a.key.localeCompare(b.key))) {
    const known = previousFailing[test.key];
    const runs = (known?.runs ?? 0) + 1;
    failing[test.key] = {runs, since: known?.since ?? build.number};
    const entry = {key: test.key, jobs: sortedLabels(test.failedIn), error: test.error, runs};
    if (known) {
      stillFailing.push(entry);
    } else {
      // A failure that is new compared with the previous run may still be one that comes and goes.
      newFailures.push({...entry, earlierRuns: previous?.history?.failed?.[test.key]?.count ?? 0});
    }
  }
  stillFailing.sort((a, b) => b.runs - a.runs || a.key.localeCompare(b.key));

  // A known failure only leaves the list on evidence: it passed in a run where every job reported,
  // it is skipped, or it is gone from a spec file that did run. Anything else (a broken job, a run
  // that left a whole configuration out) says nothing about the test, so the failure stays open.
  const filesRun = new Set(all.map(test => fileOf(test.key)));
  const complete = infra.length === 0;
  const passingAgain = [];
  const skippedWhileFailing = [];
  for (const [key, known] of Object.entries(previousFailing)) {
    const outcome = tests.get(key)?.outcome;
    if (outcome === 'failed') {
      continue;
    }
    if (outcome === 'skipped') {
      skippedWhileFailing.push(key);
      continue;
    }
    if (outcome) {
      if (complete) {
        if (known.runs >= MIN_RUNS_TO_REPORT_RECOVERY) {
          passingAgain.push(key);
        }
      } else {
        failing[key] = known;
      }
      continue;
    }
    const absent = (known.absent ?? 0) + 1;
    const removed = complete && filesRun.has(fileOf(key));
    if (!removed && absent <= MAX_ABSENT_RUNS) {
      failing[key] = {...known, absent};
    }
  }

  const keysWith = outcome => all.filter(test => test.outcome === outcome).map(test => test.key);
  const history = {
    failed: updateHistory(previous?.history?.failed, keysWith('failed')),
    flaky: updateHistory(previous?.history?.flaky, keysWith('flaky')),
  };

  return {
    schema: 1,
    build,
    hasPrevious: Boolean(previous),
    counts: {failed: count('failed'), flaky: count('flaky'), passed: count('passed'), skipped: count('skipped')},
    newFailures,
    stillFailing,
    passingAgain: passingAgain.sort(),
    skippedWhileFailing: skippedWhileFailing.sort(),
    failedStages,
    flaky: all
      .filter(test => test.outcome === 'flaky')
      .map(test => ({key: test.key, jobs: sortedLabels(test.flakyIn), runs: history.flaky[test.key].count}))
      .sort((a, b) => b.runs - a.runs || a.key.localeCompare(b.key)),
    infra,
    failing,
    history,
  };
}

function statusOf(summary) {
  const total = Object.values(summary.counts).reduce((sum, value) => sum + value, 0);
  if (total === 0) {
    return {emoji: '🔴', text: 'no E2E results were produced'};
  }
  if (summary.counts.failed > 0 || summary.infra.length > 0) {
    return {emoji: '🔴', text: 'failed'};
  }
  return summary.counts.flaky > 0 ? {emoji: '🟡', text: 'passed with flaky tests'} : {emoji: '🟢', text: 'passed'};
}

function countsLine(counts, bold) {
  const failed = `${counts.failed} failed`;
  return `${bold && counts.failed > 0 ? `*${failed}*` : failed} · ${counts.flaky} flaky · ${counts.passed} passed · ${counts.skipped} skipped`;
}

function plural(count, word) {
  return `${count} ${word}${count === 1 ? '' : 's'}`;
}

function escapeSlack(text) {
  return String(text).replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;');
}

function earlierRuns(entry) {
  return entry.earlierRuns > 0 ? ` · also failed in ${plural(entry.earlierRuns, 'earlier run')}` : '';
}

function flakyRuns(entry) {
  return entry.runs > 1 ? ` · flaky in ${entry.runs} recent runs` : '';
}

function truncate(text, limit) {
  return text.length > limit ? `${text.slice(0, limit - 1)}…` : text;
}

/** Folds failures that share an error line into one entry, so a single cause does not fill the list. */
function groupByError(entries) {
  const groups = new Map();
  for (const entry of entries) {
    groups.set(entry.error, [...(groups.get(entry.error) ?? []), entry]);
  }
  return [...groups].flatMap(([error, group]) => (error && group.length >= SLACK_GROUP_SIZE ? [{error, entries: group}] : group));
}

export function renderSlack(summary) {
  const {build} = summary;
  const status = statusOf(summary);
  const manual = build.reason && build.reason !== 'Schedule' ? ` · ${build.reason.toLowerCase()} run` : '';
  const number = build.url ? `<${build.url}|${escapeSlack(build.number)}>` : escapeSlack(build.number);
  const rerun = build.attempt > 1 ? ` · attempt ${build.attempt}` : '';
  const header = `${status.emoji} *Nightly E2E · ${escapeSlack(build.branch)}* · ${number}${manual}${rerun}`;
  // Each list is an attachment rather than a block: Slack folds a long attachment behind "Show more",
  // which keeps the message short in the channel while every list stays one click away.
  const attachments = [];
  const list = (title, items, format, total = items.length) => {
    if (items.length === 0) {
      return;
    }
    const lines = items.slice(0, SLACK_LIST_LIMIT).map(format);
    if (items.length > SLACK_LIST_LIMIT) {
      lines.push(`_+${items.length - SLACK_LIST_LIMIT} more_`);
    }
    attachments.push({
      color: SLACK_COLORS[status.emoji],
      mrkdwn_in: ['text'],
      text: truncate(`*${title} (${total})*\n${lines.join('\n')}`, SLACK_SECTION_LIMIT),
    });
  };
  const where = jobs => (jobs.length > 0 ? `  \`${jobs.join(', ')}\`` : '');
  const failureLine = entry => `• ${escapeSlack(entry.key)}${where(entry.jobs)}${earlierRuns(entry)}`;
  const errorLine = error => `      ↳ ${escapeSlack(truncate(error, 140))}`;
  const newFailureLine = item => {
    if (!item.entries) {
      return item.error ? `${failureLine(item)}\n${errorLine(item.error)}` : failureLine(item);
    }
    const jobs = sortedLabels(new Set(item.entries.flatMap(entry => entry.jobs)));
    const examples = item.entries.slice(0, SLACK_GROUP_EXAMPLES).map(entry => `      ◦ ${escapeSlack(entry.key)}`);
    if (item.entries.length > SLACK_GROUP_EXAMPLES) {
      examples.push(`      ◦ _+${item.entries.length - SLACK_GROUP_EXAMPLES} more_`);
    }
    return `• *${item.entries.length} tests with the same error*${where(jobs)}\n${errorLine(item.error)}\n${examples.join('\n')}`;
  };

  if (summary.hasPrevious) {
    list('New since the previous run', groupByError(summary.newFailures), newFailureLine, summary.newFailures.length);
    list('Still failing', summary.stillFailing, entry => `${failureLine(entry)} · ${plural(entry.runs, 'run')}`);
  } else {
    list('Failing', summary.newFailures, failureLine);
  }
  list('Passing again', summary.passingAgain, key => `• ${escapeSlack(key)}`);
  list('Skipped while failing', summary.skippedWhileFailing, key => `• ${escapeSlack(key)}`);
  list('Flaky (passed on retry)', summary.flaky, entry => `• ${escapeSlack(entry.key)}${where(entry.jobs)}${flakyRuns(entry)}`);
  list('Jobs without results', summary.infra, entry => `• ${escapeSlack(entry.job)} — ${escapeSlack(truncate(entry.reason, 140))}`);
  list('Other failed stages', summary.failedStages, entry => `• ${escapeSlack(entry.stage)} — ${escapeSlack(entry.result)}`);

  const counts = status.text === 'no E2E results were produced' ? 'No E2E results were produced.' : countsLine(summary.counts, true);
  const blocks = [{type: 'section', text: {type: 'mrkdwn', text: `${header}\n${counts}`}}];
  if (build.url) {
    blocks.push({
      type: 'context',
      elements: [{type: 'mrkdwn', text: `<${build.url}|Build> · <${build.url}&view=ms.vss-test-web.build-test-results-tab|Tests>`}],
    });
  }

  return {text: `Nightly E2E ${build.branch} (${build.number}): ${status.text}`, blocks, attachments};
}

export function renderMarkdown(summary) {
  const {build} = summary;
  const status = statusOf(summary);
  const lines = [`# ${status.emoji} Nightly E2E · ${build.branch} · ${build.number}`, '', countsLine(summary.counts, false), ''];
  const list = (title, items, format) => {
    if (items.length > 0) {
      lines.push(`## ${title} (${items.length})`, '', ...items.map(format), '');
    }
  };
  const where = jobs => (jobs.length > 0 ? ` [${jobs.join(', ')}]` : '');
  const failure = entry => `- ${entry.key}${where(entry.jobs)}${earlierRuns(entry)}${entry.error ? `\n  - ${entry.error}` : ''}`;

  if (summary.hasPrevious) {
    list('New since the previous run', summary.newFailures, failure);
    list('Still failing', summary.stillFailing, entry => `${failure(entry)}\n  - failing for ${plural(entry.runs, 'run')}`);
  } else {
    list('Failing', summary.newFailures, failure);
  }
  list('Passing again', summary.passingAgain, key => `- ${key}`);
  list('Skipped while failing', summary.skippedWhileFailing, key => `- ${key}`);
  list('Flaky (passed on retry)', summary.flaky, entry => `- ${entry.key}${where(entry.jobs)}${flakyRuns(entry)}`);
  list('Jobs without results', summary.infra, entry => `- ${entry.job}: ${entry.reason}`);
  list('Other failed stages', summary.failedStages, entry => `- ${entry.stage}: ${entry.result}`);

  return lines.join('\n');
}

function readBuildFromEnvironment(env) {
  const collection = env.SYSTEM_COLLECTIONURI ?? '';
  const project = env.SYSTEM_TEAMPROJECT ?? '';
  const id = env.BUILD_BUILDID ?? '';
  return {
    id,
    number: env.BUILD_BUILDNUMBER ?? 'local',
    branch: (env.BUILD_SOURCEBRANCH ?? 'local').replace(/^refs\/heads\//, ''),
    commit: env.BUILD_SOURCEVERSION ?? '',
    reason: env.BUILD_REASON ?? '',
    attempt: Number(env.SYSTEM_JOBATTEMPT) || 1,
    url: collection && id ? `${collection}${encodeURIComponent(project)}/_build/results?buildId=${id}` : '',
  };
}

async function fetchTimeline(env) {
  const collection = env.SYSTEM_COLLECTIONURI;
  const token = env.SYSTEM_ACCESSTOKEN;
  if (!collection || !token || !env.BUILD_BUILDID) {
    return null;
  }
  const url = `${collection}${encodeURIComponent(env.SYSTEM_TEAMPROJECT)}/_apis/build/builds/${env.BUILD_BUILDID}/timeline?api-version=7.1`;
  try {
    const response = await fetch(url, {headers: {Authorization: `Bearer ${token}`}});
    if (!response.ok) {
      console.warn(`Could not read the build timeline (HTTP ${response.status}); skipping missing-job detection.`);
      return null;
    }
    return await response.json();
  } catch (error) {
    console.warn(`Could not read the build timeline (${error.message}); skipping missing-job detection.`);
    return null;
  }
}

/** Loads the summary of the previous run; a run that was retried holds one per attempt and the latest counts. */
export async function loadPrevious(dir, build) {
  let previous = null;
  for (const file of await findFiles(dir, 'summary.json')) {
    try {
      const candidate = await readJson(file);
      if (!previous || (candidate.build?.attempt ?? 1) > (previous.build?.attempt ?? 1)) {
        previous = candidate;
      }
    } catch {
      console.warn(`Could not read ${file}; ignoring that summary.`);
    }
  }
  return previous && build.id && String(previous.build?.id ?? '') === String(build.id) ? null : previous;
}

async function postToSlack(webhook, payload) {
  if (!/^https:\/\//.test(webhook ?? '')) {
    console.warn('No Slack webhook configured; skipping the notification.');
    return;
  }
  try {
    const response = await fetch(webhook, {
      method: 'POST',
      headers: {'Content-Type': 'application/json'},
      body: JSON.stringify(payload),
    });
    if (!response.ok) {
      console.warn(`Slack rejected the notification (HTTP ${response.status}): ${await response.text()}`);
    }
  } catch (error) {
    console.warn(`Could not reach Slack (${error.message}).`);
  }
}

async function main() {
  const {values} = parseArgs({
    options: {
      summaries: {type: 'string'},
      previous: {type: 'string'},
      out: {type: 'string'},
      timeline: {type: 'string'},
      'dry-run': {type: 'boolean', default: false},
    },
  });
  if (!values.summaries || !values.out) {
    throw new Error('Both --summaries and --out are required.');
  }

  const build = readBuildFromEnvironment(process.env);
  const jobs = await loadJobs(values.summaries);
  const timeline = values.timeline ? await readJson(values.timeline) : await fetchTimeline(process.env);
  const {tests, infra} = aggregate(jobs);
  infra.push(...findMissingJobs(timeline, jobs));

  const summary = buildSummary({
    tests,
    infra,
    previous: await loadPrevious(values.previous, build),
    build,
    failedStages: findFailedStages(timeline),
  });
  const markdown = renderMarkdown(summary);
  const slack = renderSlack(summary);

  await mkdir(values.out, {recursive: true});
  await writeFile(path.join(values.out, 'summary.json'), JSON.stringify(summary, null, 2));
  await writeFile(path.join(values.out, 'summary.md'), markdown);
  console.log(markdown);

  if (values['dry-run']) {
    console.log(JSON.stringify(slack, null, 2));
  } else {
    await postToSlack(process.env.SLACK_WEBHOOK_URL, slack);
  }
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  try {
    await main();
  } catch (error) {
    // A silent channel reads as a clean night, so a summary that could not be produced is announced as well.
    if (!process.argv.includes('--dry-run')) {
      const build = readBuildFromEnvironment(process.env);
      const link = build.url ? ` <${build.url}|Build>` : '';
      await postToSlack(process.env.SLACK_WEBHOOK_URL, {
        text: `🔴 Nightly E2E ${build.branch} (${build.number}): the summary could not be produced — ${error.message}${link}`,
      });
    }
    throw error;
  }
}
