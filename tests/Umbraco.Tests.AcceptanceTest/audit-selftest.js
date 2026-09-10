/**
 * Self-test for audit-conventions.js.
 *
 * The audit gates CI, and over half its rules have a budget of 0. For those, a rule whose
 * regex silently stops matching looks exactly like a rule that is passing — it reports
 * "clean" forever while the convention goes unenforced. Two of these detectors had real
 * bugs when first written (a dropped-promise rule that flagged 19 false positives, and a
 * commented-assertion rule that double-counted a wholly-dead file), so this is a
 * demonstrated failure mode, not a theoretical one.
 *
 * Each case below builds a tiny fixture tree, points the audit at it with --root, and
 * asserts the expected count. `bad` proves the rule still detects; `good` proves it does
 * not fire on the idiom it is supposed to allow.
 *
 * Run: npm run audit:selftest
 */
const fs = require('fs');
const os = require('os');
const path = require('path');
const {execFileSync} = require('child_process');

const CASES = [
  {
    rule: 'droppedPromise', expect: 2,
    files: {
      'tests/DefaultConfig/a.spec.ts': [
        `values.forEach(async v => { await ui.doesRenderValueContainText(v); });`,
        `expect(locator).toBeVisible();`,
      ],
    },
  },
  {
    // the false positive that bit once: a multi-line `await expect(...).toPass()` closes
    // with a bare `}).toPass({...})`, which is not a dropped promise
    rule: 'droppedPromise', expect: 0, label: 'multi-line await expect().toPass()',
    files: {
      'tests/DefaultConfig/b.spec.ts': [
        `await expect(async () => {`,
        `  await this.click(btn);`,
        `}).toPass({timeout: ConstantHelper.timeout.medium});`,
      ],
    },
  },
  {
    rule: 'disabledWithoutAnnotation', expect: 1,
    files: {'tests/DefaultConfig/c.spec.ts': [`test.skip('x', async ({umbracoUi}) => {});`]},
  },
  {
    rule: 'disabledWithoutAnnotation', expect: 0, label: 'annotated skip is allowed',
    files: {
      'tests/DefaultConfig/d.spec.ts': [
        `test.skip('x', {annotation: {type: 'issue', description: 'y'}}, async ({umbracoUi}) => {});`,
      ],
    },
  },
  {
    // expect() on a promise is always truthy, so the assertion can never fail
    rule: 'promiseInExpect', expect: 1,
    files: {
      'lib/helpers/QApiHelper.ts': [
        `export class QApiHelper {`,
        `  async isThingVisible(name: string) {`,
        `    await this.isVisible(this.thing);`,
        `  }`,
        `}`,
      ],
      'tests/DefaultConfig/x.spec.ts': [`  expect(umbracoUi.thing.isThingVisible('x')).toBeTruthy();`],
    },
  },
  {
    // the synchronous getters are *meant* to be wrapped in expect() - firing on those would
    // flag the correct form and get the rule switched off
    rule: 'promiseInExpect', expect: 0, label: 'a synchronous getter in expect() is correct',
    files: {
      'lib/helpers/Q2ApiHelper.ts': [
        `export class Q2ApiHelper {`,
        `  getOnlyPropertyValue(data: any): any {`,
        `    return data.values[0].value;`,
        `  }`,
        `}`,
      ],
      'tests/DefaultConfig/y.spec.ts': [`  expect(umbracoApi.document.getOnlyPropertyValue(contentData)).toBe('x');`],
    },
  },
  {
    rule: 'promiseInExpect', expect: 0, label: 'await inside expect() is correct',
    files: {
      'lib/helpers/Q3ApiHelper.ts': [
        `export class Q3ApiHelper {`,
        `  async doesNameExist(name: string) {`,
        `    return await this.getByName(name);`,
        `  }`,
        `}`,
      ],
      'tests/DefaultConfig/z.spec.ts': [`  expect(await umbracoApi.document.doesNameExist('x')).toBeTruthy();`],
    },
  },
  {
    rule: 'entityNotTornDown', expect: 1, label: 'a language is created and never removed',
    files: {
      'tests/DefaultConfig/u.spec.ts': [
        `  await umbracoApi.language.createDanishLanguage();`,
        `  await umbracoApi.document.ensureNameNotExists(contentName);`,
      ],
    },
  },
  {
    rule: 'entityNotTornDown', expect: 0, label: 'explicit teardown is allowed',
    files: {
      'tests/DefaultConfig/v.spec.ts': [
        `  await umbracoApi.language.createDanishLanguage();`,
        `  await umbracoApi.language.ensureNameNotExists('Danish');`,
      ],
    },
  },
  {
    // deleting a type removes its instances, so a document needs no teardown of its own when
    // the document type is cleaned - without this the rule fires on most specs in the suite
    rule: 'entityNotTornDown', expect: 0, label: 'a document cascades from its document type',
    files: {
      'tests/DefaultConfig/w.spec.ts': [
        `  await umbracoApi.document.createDefaultDocument(contentName, documentTypeId);`,
        `  await umbracoApi.documentType.ensureNameNotExists(documentTypeName);`,
      ],
    },
  },
  {
    rule: 'orphanSpec', expect: 1,
    files: {'tests/e.spec.ts': [`test('x', async () => {});`]},
  },
  {
    rule: 'rawFixtureInSpec', expect: 1,
    files: {'tests/DefaultConfig/f.spec.ts': [`await page.locator('#x').click();`]},
  },
  {
    rule: 'helperPageInSpec', expect: 1, label: 'through-helper page is a separate, milder rule',
    files: {'tests/DefaultConfig/g.spec.ts': [`await umbracoUi.page.goto('/x');`]},
  },
  {
    rule: 'hardcodedEndpoint', expect: 1,
    files: {'tests/DefaultConfig/h.spec.ts': [`resp.url().includes('/umbraco/management/api/v1/telemetry/level')`]},
  },
  {
    rule: 'commentedOutTest', expect: 1,
    files: {'tests/DefaultConfig/i.spec.ts': [`test('live', async () => {});`, `// test('dead', async () => {});`]},
  },
  {
    rule: 'commentedOutAssertion', expect: 1,
    files: {'tests/DefaultConfig/j.spec.ts': [`test('live', async () => {});`, `// await umbracoUi.content.isX();`]},
  },
  {
    // the double-count that bit once: in a file with no live test, commented assertions
    // belong to commentedOutTest, not to commentedOutAssertion
    rule: 'commentedOutAssertion', expect: 0, label: 'wholly-dead file counts as commentedOutTest only',
    files: {'tests/DefaultConfig/k.spec.ts': [`// test('dead', async () => {});`, `// await umbracoUi.content.isX();`]},
  },
  {
    rule: 'unanchoredTodo', expect: 1,
    files: {'tests/DefaultConfig/l.spec.ts': [`// TODO: implement it later`]},
  },
  {
    rule: 'unanchoredTodo', expect: 0, label: 'anchored TODOs are allowed',
    files: {
      'tests/DefaultConfig/m.spec.ts': [
        `// TODO (V19): remove once the obsolete overload is gone`,
        `// TODO: pagination [NL]`,
      ],
    },
  },
  {
    rule: 'rawResponseAssertion', expect: 1,
    files: {
      'tests/DefaultConfig/n.spec.ts': [
        `  const contentData = await umbracoApi.document.getByName(contentName);`,
        `  expect(contentData.variants[0].state).toBe('Published');`,
      ],
    },
  },
  {
    // the rule keys on provenance: a *Data variable built locally is not a response shape
    rule: 'rawResponseAssertion', expect: 0, label: 'a locally-built *Data object is not a response',
    files: {
      'tests/DefaultConfig/n2.spec.ts': [
        `  const exportData = await umbracoUi.dictionary.exportDictionaryAndReadFile(true);`,
        `  expect(exportData.content).toContain('Name="x"');`,
      ],
    },
  },
  {
    rule: 'rawResponseAssertion', expect: 0, label: 'the helper form is allowed',
    files: {'tests/DefaultConfig/o.spec.ts': [`  await umbracoApi.document.doesVariantHaveState(contentData, 'Published');`]},
  },
  {
    rule: 'fixedSleep', expect: 1,
    files: {'lib/helpers/X.ts': [`    await this.page.waitForTimeout(ConstantHelper.wait.short);`]},
  },
  {
    // §3 permits a sleep with a justification, so counting justified ones would make the
    // convention unfollowable - a properly justified addition would fail the build.
    rule: 'fixedSleep', expect: 0, label: 'a justified sleep is allowed',
    files: {'lib/helpers/X2.ts': [`    await this.page.waitForTimeout(ConstantHelper.wait.minimal); // revealed elements are not found without this`]},
  },
  {
    rule: 'forceClick', expect: 1,
    files: {'lib/helpers/Y.ts': [`    await this.click(btn, {force: true});`]},
  },
  {
    rule: 'forceClick', expect: 0, label: 'a justified force click is allowed',
    files: {'lib/helpers/Y2.ts': [`    await this.click(btn, {force: true}); // overlay is decorative and never intercepts`]},
  },
  {
    rule: 'substringEntityName', expect: 1,
    files: {'lib/helpers/Z.ts': [`    return this.mediaCardItems.filter({hasText: mediaName});`]},
  },
  {
    rule: 'substringEntityName', expect: 0, label: 'structural label filtering is allowed',
    files: {'lib/helpers/Z2.ts': [`    return this.box.filter({hasText: 'Document permissions'});`]},
  },
  {
    rule: 'literalIndexLocator', expect: 2, label: 'multi-digit literals count too',
    files: {'lib/helpers/W.ts': [`    return this.property.nth(0);`, `    return this.property.nth(12);`]},
  },
  {
    // "the i-th block" with a parameterised index is the legitimate form (§3)
    rule: 'literalIndexLocator', expect: 0, label: 'a variable index is allowed',
    files: {'lib/helpers/W2.ts': [`    return this.blockGridEntry.nth(index);`]},
  },
  {
    rule: 'hardcodedTimeout', expect: 1,
    files: {'lib/helpers/V.ts': [`    await expect(x).toBeVisible({timeout: 15000});`]},
  },
  {
    // The shape worth flagging: an inline object literal with no annotation. TypeScript's
    // excess-property check would name a misspelled field here if the return type were declared.
    rule: 'untypedBuilderExit', expect: 1,
    files: {
      'lib/builders/q/qBuilder.ts': [
        `export class QBuilder {`,
        `  build() {`,
        `    return {`,
        `      alias: this.alias || null,`,
        `      value: this.value || null`,
        `    };`,
        `  }`,
        `}`,
      ],
    },
  },
  {
    rule: 'untypedBuilderExit', expect: 0, label: 'a typed exit is allowed',
    files: {'lib/builders/r/rBuilder.ts': [`export class RBuilder {`, `  getValues(): DataTypeValues {`, `    return [];`, `  }`, `}`]},
  },
  {
    // An annotated local is what actually catches a misspelled field - an unannotated
    // `const values = []` infers `any[]`, and `any[]` satisfies a `DataTypeValues` return
    // annotation, so requiring the return type here would buy nothing.
    rule: 'untypedBuilderExit', expect: 0, label: 'an annotated local that is returned is already checked',
    files: {
      'lib/builders/r2/r2Builder.ts': [
        `export class R2Builder {`,
        `  getValues() {`,
        `    const values: DataTypeValues = [];`,
        `    values.push({alias: 'items', value: this.items});`,
        `    return values;`,
        `  }`,
        `}`,
      ],
    },
  },
  {
    rule: 'untypedBuilderExit', expect: 0, label: 'an empty literal has no shape to get wrong',
    files: {'lib/builders/r3/r3Builder.ts': [`export class R3Builder {`, `  getValues() {`, `    return [];`, `  }`, `}`]},
  },
  {
    // getValue() is an exit too - it was outside the rule until the rule was keyed on effect.
    rule: 'untypedBuilderExit', expect: 1, label: 'getValue() counts as an exit',
    files: {
      'lib/builders/r4/r4Builder.ts': [
        `export class R4Builder {`,
        `  getValue() {`,
        `    return {`,
        `      contentKey: this.contentKey`,
        `    };`,
        `  }`,
        `}`,
      ],
    },
  },
  {
    // `return buildProperty({...})` is checked at the call site - excess-property checking
    // applies to an argument literal, and BuilderUtils declares a return type on each of these.
    rule: 'untypedBuilderExit', expect: 0, label: 'a pass-through to a typed utility is already checked',
    files: {
      'lib/builders/r6/r6Builder.ts': [
        `export class R6Builder {`,
        `  build() {`,
        `    return buildProperty({`,
        `      id: this.id,`,
        `      alias: this.alias`,
        `    });`,
        `  }`,
        `}`,
      ],
    },
  },
  {
    // The exemption must not swallow an exit that builds a literal and calls something in it.
    rule: 'untypedBuilderExit', expect: 1, label: 'a literal containing a call is still counted',
    files: {
      'lib/builders/r7/r7Builder.ts': [
        `export class R7Builder {`,
        `  build() {`,
        `    return {`,
        `      alias: this.alias,`,
        `      value: this.inner.getValue()`,
        `    };`,
        `  }`,
        `}`,
      ],
    },
  },
  {
    // An `any`-typed local is not "already checked" - it must still be counted.
    rule: 'untypedBuilderExit', expect: 1, label: 'an any-typed local is not a check',
    files: {
      'lib/builders/r5/r5Builder.ts': [
        `export class R5Builder {`,
        `  getValues() {`,
        `    let values: any = {};`,
        `    values.label = this.label;`,
        `    return values;`,
        `  }`,
        `}`,
      ],
    },
  },
  {
    rule: 'anyInBuilder', expect: 1,
    files: {'lib/builders/s/sBuilder.ts': [`  const values: any[] = [];`]},
  },
  {
    rule: 'deprecationWithoutRemoval', expect: 1,
    files: {'lib/helpers/D.ts': [`  /** @deprecated Prefer {@link other}. */`]},
  },
  {
    rule: 'deprecationWithoutRemoval', expect: 0, label: 'a removal version on a later JSDoc line counts',
    files: {
      'lib/helpers/D2.ts': [
        `  /**`,
        `   * @deprecated Prefer {@link other}.`,
        `   * Scheduled for removal in 20.0.`,
        `   */`,
      ],
    },
  },
  {
    // the dangerous shape: an ignored flag means `false` still asserts the positive
    rule: 'unusedHelperParam', expect: 1, label: 'ignored isVisible flag',
    files: {
      'lib/helpers/PApiHelper.ts': [
        `export class PApiHelper {`,
        `  async isThingVisible(name: string, isVisible: boolean = true) {`,
        `    await this.isVisible(this.thing.filter({hasText: name}));`,
        `  }`,
        `}`,
      ],
    },
  },
  {
    rule: 'unusedHelperParam', expect: 0, label: 'a used parameter is allowed',
    files: {
      'lib/helpers/P2ApiHelper.ts': [
        `export class P2ApiHelper {`,
        `  async isThingVisible(name: string, isVisible: boolean = true) {`,
        `    await this.isVisible(this.thing.filter({hasText: name}), isVisible);`,
        `  }`,
        `}`,
      ],
    },
  },
  {
    // spread must survive the member-access stripping, or a used parameter reads as unused
    rule: 'unusedHelperParam', expect: 0, label: 'a parameter used via spread is allowed',
    files: {
      'lib/helpers/P4ApiHelper.ts': [
        `export class P4ApiHelper {`,
        `  async moveAll(mediaIds: string[], extraHeaders?: object) {`,
        `    const all = {...this.base, ...extraHeaders};`,
        `    await Promise.all([...mediaIds.map((id) => this.wait(id)), this.click()]);`,
        `  }`,
        `}`,
      ],
    },
  },
  {
    rule: 'unusedHelperParam', expect: 0, label: 'control flow is not a method signature',
    files: {
      'lib/helpers/P3ApiHelper.ts': [
        `export class P3ApiHelper {`,
        `  async doThing(name: string) {`,
        `    if (name) {`,
        `      return name;`,
        `    }`,
        `  }`,
        `}`,
      ],
    },
  },
  {
    rule: 'rawClickInLib', expect: 1,
    files: {'lib/helpers/U.ts': [`    await this.saveBtn.click();`]},
  },
  {
    rule: 'rawClickInLib', expect: 0, label: 'this.click() is the wrapper, not a raw click',
    files: {'lib/helpers/U2.ts': [`    await this.click(this.saveBtn);`]},
  },
  {
    // asserts internally + no await: the assertions still run, but a failure becomes an
    // unhandled rejection and Playwright reports the test as PASSED. Verified by probe.
    rule: 'unawaitedAssertion', expect: 1,
    files: {
      'lib/helpers/SApiHelper.ts': [
        `export class SApiHelper {`,
        `  async doesThingMatch(name: string) {`,
        `    await this.isVisible(this.thing);`,
        `  }`,
        `}`,
      ],
      'tests/DefaultConfig/r.spec.ts': [`  umbracoApi.thing.doesThingMatch('x');`],
    },
  },
  {
    // `verify*` is the fourth assertion prefix in lib/ (composite checks in the Delivery API
    // helpers). It has the same async-body-with-sync-assertions shape, so it needs the same rule.
    rule: 'unawaitedAssertion', expect: 1, label: 'verify* prefix is covered too',
    files: {
      'lib/helpers/VApiHelper.ts': [
        `export class VApiHelper {`,
        `  async verifyBasicProperties(json: any) {`,
        `    expect(json.name).toBe('x');`,
        `  }`,
        `}`,
      ],
      'tests/DefaultConfig/t.spec.ts': [`  umbracoApi.contentDeliveryApi.verifyBasicProperties(json);`],
    },
  },
  {
    rule: 'unawaitedAssertion', expect: 0, label: 'awaited is correct',
    files: {
      'lib/helpers/S2ApiHelper.ts': [
        `export class S2ApiHelper {`,
        `  async doesThingMatch(name: string) {`,
        `    await this.isVisible(this.thing);`,
        `  }`,
        `}`,
      ],
      'tests/DefaultConfig/s.spec.ts': [`  await umbracoApi.thing.doesThingMatch('x');`],
    },
  },
  {
    // a does* that only returns a value, bare-awaited in a spec -> the assertion evaporates
    rule: 'discardedCheck', expect: 1,
    files: {
      'lib/helpers/TApiHelper.ts': [
        `export class TApiHelper {`,
        `  async doesThingExist(name: string) {`,
        `    return await this.get(name);`,
        `  }`,
        `}`,
      ],
      'tests/DefaultConfig/p.spec.ts': [`  await umbracoApi.thing.doesThingExist('x');`],
    },
  },
  {
    rule: 'discardedCheck', expect: 0, label: 'wrapped in expect() is correct',
    files: {
      'lib/helpers/T2ApiHelper.ts': [
        `export class T2ApiHelper {`,
        `  async doesThingExist(name: string) {`,
        `    return await this.get(name);`,
        `  }`,
        `}`,
      ],
      'tests/DefaultConfig/q.spec.ts': [`  expect(await umbracoApi.thing.doesThingExist('x')).toBeTruthy();`],
    },
  },
];

const tmp = fs.mkdtempSync(path.join(os.tmpdir(), 'audit-selftest-'));
let failed = 0, ran = 0;

function runAudit(root) {
  let out;
  try {
    out = execFileSync(process.execPath, [path.join(__dirname, 'audit-conventions.js'), `--root=${root}`, '--verbose'], {encoding: 'utf8'});
  } catch (e) {
    out = (e.stdout || '') + (e.stderr || '');   // non-zero exit is expected when over budget
  }
  const counts = {};
  for (const line of out.split(/\r?\n/)) {
    // "  <label>   <found>   <budget>   <status>"
    const m = line.match(/^ {2}\S.*?\s{2,}(\d+)\s+(\d+)\s+\S/);
    if (m) counts[line.trim().split(/\s{2,}/)[0]] = Number(m[1]);
  }
  return {out, counts};
}

// map rule key -> printed label, read from the audit itself so the two cannot drift
const LABELS = (() => {
  const src = fs.readFileSync(path.join(__dirname, 'audit-conventions.js'), 'utf8');
  const block = src.slice(src.indexOf('const RULES = ['), src.indexOf('];', src.indexOf('const RULES = [')));
  const map = {};
  for (const m of block.matchAll(/\['([a-zA-Z]+)',\s*'([^']+)'\]/g)) map[m[1]] = m[2];
  return map;
})();

for (const c of CASES) {
  ran++;
  const root = fs.mkdtempSync(path.join(tmp, 'case-'));
  for (const [rel, lines] of Object.entries(c.files)) {
    const p = path.join(root, rel);
    fs.mkdirSync(path.dirname(p), {recursive: true});
    fs.writeFileSync(p, lines.join('\n') + '\n');
  }
  const {counts} = runAudit(root);
  const label = LABELS[c.rule];
  if (!label) { console.error(`  FAIL  ${c.rule}: not present in the audit's RULES list`); failed++; continue; }
  const got = counts[label] ?? 0;
  const what = `${c.rule}${c.label ? ` (${c.label})` : ''}`;
  if (got !== c.expect) {
    console.error(`  FAIL  ${what}: expected ${c.expect}, got ${got}`);
    failed++;
  } else {
    console.log(`  ok    ${what} -> ${got}`);
  }
}

// every rule the audit reports must be covered by at least one case
const covered = new Set(CASES.map(c => c.rule));
// endpointNotInOpenApi is exercised against the real repo instead of a fixture: it reads
// OpenApi.json two directories above --root, which this harness does not synthesise. It was
// verified by renaming a constant in the live tree and seeing the rule fire.
const NO_FIXTURE = ['endpointNotInOpenApi'];
const uncovered = Object.keys(LABELS).filter(k => !covered.has(k) && !NO_FIXTURE.includes(k));
if (uncovered.length) {
  console.error(`\n  FAIL  no self-test case for: ${uncovered.join(', ')}`);
  failed += uncovered.length;
}

// The rule table in CLAUDE.md §7 documents why each rule exists. It used to restate each
// budget too, and four of those numbers had drifted from the real ones - so the numbers were
// removed and what remains is whether a rule is a gate (budget 0) or ratcheted debt. That still
// drifts if a rule is added, or flips between the two, without the table following. Checked
// here by count rather than by matching prose to rule names, which would just move the drift
// into a third place.
{
  const budgetBlock = fs.readFileSync(path.join(__dirname, 'audit-conventions.js'), 'utf8')
    .match(/const BUDGET = \{[\s\S]*?\n\};/)[0];
  const budgets = [...budgetBlock.matchAll(/^\s*([a-zA-Z]+):\s*(\d+),/gm)].map(m => [m[1], Number(m[2])]);
  const gates = budgets.filter(([, n]) => n === 0).length;
  const debt = budgets.length - gates;

  const md = fs.readFileSync(path.join(__dirname, 'CLAUDE.md'), 'utf8');
  const rows = [...md.matchAll(/^\| [^|]+ \| (gate|debt) \|/gm)].map(m => m[1]);
  const docGates = rows.filter(r => r === 'gate').length;
  const docDebt = rows.filter(r => r === 'debt').length;

  if (rows.length !== budgets.length || docGates !== gates || docDebt !== debt) {
    console.error(`\n  FAIL  CLAUDE.md §7 table is out of step with BUDGET: ` +
      `table has ${rows.length} rows (${docGates} gate, ${docDebt} debt), ` +
      `BUDGET has ${budgets.length} rules (${gates} gate, ${debt} debt)`);
    failed++;
  } else {
    console.log(`  ok    CLAUDE.md §7 table covers all ${budgets.length} rules (${gates} gate, ${debt} debt)`);
  }
}

fs.rmSync(tmp, {recursive: true, force: true});
console.log(`\n${ran} case(s), ${failed} failure(s)`);
process.exit(failed ? 1 : 0);
