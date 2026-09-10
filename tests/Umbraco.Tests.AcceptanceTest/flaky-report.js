/**
 * Reads a Playwright JSON report and names two things the suite otherwise hides.
 *
 * 1. FLAKY tests. `retries: 2` means a test that fails twice and passes on the third attempt
 *    is reported as green, and the JUnit report CI publishes exposes only the final result.
 *    So the flaky set has been invisible: known as folklore, never as a list.
 *
 * 2. SLOW tests. The per-test timeout is 60s. A test finishing at 55s passes on a quiet agent
 *    and fails on a busy one, which is a flake with a different cause. Nothing surfaced these.
 *
 * Usage:
 *   npm run flaky                       # reads results/results.json
 *   npm run flaky -- path/to/report.json
 *
 * CI writes results/results.json (see playwright.config.ts) and publishes results/ as the
 * "Acceptance Test Results" pipeline artifact, so download a nightly one and point this at it
 * to see which tests are flaky over time rather than just tonight.
 */
const fs = require('fs');
const path = require('path');

const TIMEOUT_MS = 60_000;          // playwright.config.ts: timeout
const SLOW_FRACTION = 0.5;          // flag anything past half the budget

const reportPath = process.argv[2] || path.join('results', 'results.json');

if (!fs.existsSync(reportPath)) {
  console.error(`No report at ${reportPath}.`);
  console.error('Produce one with:  npx playwright test <target>');
  console.error('or point this at a nightly artifact:  npm run flaky -- <path>');
  process.exit(2);
}

let report;
try {
  report = JSON.parse(fs.readFileSync(reportPath, 'utf8'));
} catch (e) {
  console.error(`${reportPath} is not valid JSON: ${e.message}`);
  process.exit(2);
}

const tests = [];
const walk = suite => {
  (suite.suites || []).forEach(walk);
  for (const spec of suite.specs || []) {
    for (const t of spec.tests || []) {
      tests.push({
        title: spec.title,
        file: spec.file || suite.file || '?',
        line: spec.line ?? 0,
        status: t.status,
        results: t.results || [],
      });
    }
  }
};
(report.suites || []).forEach(walk);

if (!tests.length) {
  // An empty `suites` is a real report of a run that matched nothing; a missing one is not a report.
  const message = Array.isArray(report.suites)
    ? `${reportPath} is a report of a run with no tests - did --grep match nothing?`
    : `${reportPath} has no "suites" - is it a Playwright JSON report?`;
  console.error(message);
  process.exit(2);
}

const attemptsOf = t => t.results.length;
// Worst attempt, not the last one: a test whose failing attempt hit the ceiling is the one at risk.
const worstDuration = t => Math.max(0, ...t.results.map(r => r.duration || 0));
// Every attempt, because retries are wall-clock the run actually spent.
const totalDuration = t => t.results.reduce((sum, r) => sum + (r.duration || 0), 0);
const ranAtAll = t => attemptsOf(t) > 0 && t.status !== 'skipped';

// A test that needed more than one attempt and ended up passing is flaky by definition,
// whatever the final report says.
const flaky = tests
  .filter(t => attemptsOf(t) > 1 && t.results[t.results.length - 1].status === 'passed')
  .sort((a, b) => attemptsOf(b) - attemptsOf(a));

const failed = tests.filter(t => ['unexpected', 'failed'].includes(t.status));
const ran = tests.filter(ranAtAll);
// Skipped tests carry a duration too, and it means nothing - don't call them slow.
const slow = ran
  .filter(t => worstDuration(t) >= TIMEOUT_MS * SLOW_FRACTION)
  .sort((a, b) => worstDuration(b) - worstDuration(a));

const totalMs = ran.reduce((sum, t) => sum + totalDuration(t), 0);

const loc = t => `${t.file}:${t.line}`;
const secs = ms => (ms / 1000).toFixed(1) + 's';

console.log(`\n${reportPath}`);
console.log(`  tests in report : ${tests.length}   ran: ${ran.length}   wall-clock in tests: ${secs(totalMs)}`);
console.log(`  flaky           : ${flaky.length}`);
console.log(`  failed          : ${failed.length}`);
console.log(`  over ${SLOW_FRACTION * 100}% of the ${TIMEOUT_MS / 1000}s timeout : ${slow.length}`);

if (flaky.length) {
  console.log('\nFLAKY - passed only after a retry. These are the ones to fix first:');
  for (const t of flaky) {
    const pattern = t.results.map(r => r.status === 'passed' ? 'pass' : 'FAIL').join(' -> ');
    console.log(`  ${loc(t)}\n      ${t.title}\n      ${attemptsOf(t)} attempts: ${pattern}`);
  }
}

if (failed.length) {
  console.log('\nFAILED after every retry:');
  for (const t of failed) console.log(`  ${loc(t)}\n      ${t.title}`);
}

if (slow.length) {
  console.log(`\nSLOW - a test near the ${TIMEOUT_MS / 1000}s limit passes on a quiet agent and fails on a busy one:`);
  for (const t of slow.slice(0, 20)) {
    console.log(`  ${secs(worstDuration(t)).padStart(7)}  ${loc(t)}\n           ${t.title}`);
  }
  if (slow.length > 20) console.log(`  ... and ${slow.length - 20} more`);
}

if (!flaky.length && !failed.length && !slow.length) {
  console.log('\nNothing flaky, failing, or close to the timeout in this report.');
}

// Reporting is not a gate: a flaky test is information, and failing the build on it here would
// duplicate what the run itself already reported.
console.log('');
