/**
 * Audits the suite against the conventions in CLAUDE.md §3.
 *
 * There is no lint step for this project, so these rules were previously applied by hand.
 * This is the checker: `npm run audit` prints a count per rule and exits non-zero when a
 * rule with a budget of 0 is violated. Rules with a non-zero budget are known debt — the
 * budget is the current count, so it can only shrink.
 *
 * Needs no Umbraco instance.
 */
const fs = require('fs');
const path = require('path');

// Known debt. Lower these as the suite is cleaned up; never raise one to make a build pass.
const BUDGET = {
  droppedPromise: 0,
  discardedCheck: 0,
  orphanSpec: 0,
  rawFixtureInSpec: 0,
  hardcodedEndpoint: 0,
  disabledWithoutAnnotation: 0,
  fixedSleep: 81,
  forceClick: 79,
  // Paid off: 14 -> 0. Media cards match the `name` attribute (bound as an attribute, and a
  // pre-existing usage in this repo verified the form); block-type, user and search-result
  // items match exact text instead, because those bind `.name=` as a property and nothing
  // reflects it - so no attribute exists there and `[name="..."]` would match nothing.
  substringEntityName: 0,
  // Paid off: 5 -> 0. Four were false positives of a rule that keyed on the spelling of the
  // call rather than on whether visibility was awaited; the fifth was real and is fixed.
  rawClickInLib: 0,
  literalIndexLocator: 15,
  hardcodedTimeout: 0,
  // Paid off: 11 -> 0. Broadening the pattern from a list of page methods to any page member
  // first raised it to 17 (six were invisible, two of them on the lines either side of one it
  // did flag), then all 17 moved behind helpers: getCurrentUrl, waitForUrl,
  // goToEntityWorkspace on UiHelpers, and waitForTimeout on ApiHelpers.
  helperPageInSpec: 0,
  // Rebased 96 -> 73 when the rule was keyed on effect rather than on the presence of an
  // annotation. 36 of the old 96 were DataTypeBuilder subclasses whose `const values:
  // DataTypeValues = []` already catches a misspelled field, where a return type provably
  // changes nothing; 13 `getValue()` exits that do return an unchecked shape were outside the
  // old rule and are now counted. No code changed - the number was wrong, not the suite.
  //   Then 73 -> 64: nine more were `return buildProperty(...)` / `buildContainer` /
  //   `buildComposition` pass-throughs, which BuilderUtils already types at the call site.
  //   Then 64 -> 44: the variant, value, entity and content-type builders now declare their
  //   payload types (lib/builders/types.ts). What is left is the sub-builders - block, tiptap,
  //   list-view, user-group permissions - each of which needs an interface for its item shape.
  // Paid off: every builder exit now declares its payload type, so this is a gate rather than
  // debt. A new exit returning an unchecked shape fails the build.
  untypedBuilderExit: 0,
  // 25 -> 12. The 13 removed were `let values: any = {}` accumulators, now declared with the
  // shape they build - which is what makes a misspelled field an error rather than silence.
  // Of the 12 left, nine are correct: a property `value` is genuinely heterogeneous, and the
  // four `let value: any = null` accumulators hold one. The other three are exported
  // signatures that should narrow but cannot in a minor - each carries a TODO (V19).
  anyInBuilder: 12,
  commentedOutTest: 0,
  commentedOutAssertion: 15,
  unanchoredTodo: 15,
  // 190 + 11. The 11 are CreatedPackages.spec.ts, which was 347 lines of commented-out tests
  // and is now uncommented as annotated fixmes: its debt was always present, just invisible to
  // a scanner that skips comments. This is the one legitimate reason to raise a budget - debt
  // becoming countable, not a new violation - and it is recorded here so it cannot be mistaken
  // for the forbidden move of raising a budget to get a run green.
  rawResponseAssertion: 201,
  deprecationWithoutRemoval: 0,
  unusedHelperParam: 1,
  unawaitedAssertion: 0,
  endpointNotInOpenApi: 0,
  entityNotTornDown: 0,
  promiseInExpect: 0,
};

// --root lets audit-selftest.js point the same rules at a fixture tree. Without a way to
// run the rules against known-bad input, a rule whose regex stops matching is
// indistinguishable from a rule that is passing - it just reports "clean" forever.
const rootArg = process.argv.find(a => a.startsWith('--root='));
const ROOT = rootArg ? rootArg.slice('--root='.length) : '.';
const at = p => path.join(ROOT, p);

const walk = (d, test, acc = []) => {
  if (!fs.existsSync(d)) return acc;
  for (const e of fs.readdirSync(d, {withFileTypes: true})) {
    if (e.name === 'node_modules' || e.name === 'dist') continue;
    const p = path.join(d, e.name);
    if (e.isDirectory()) walk(p, test, acc);
    else if (test(e.name)) acc.push(p);
  }
  return acc;
};

const rel = p => path.relative(ROOT, p).split(path.sep).join('/');
const read = f => fs.readFileSync(f, 'utf8').split(/\r?\n/);
const isComment = l => /^\s*(\/\/|\*|\/\*)/.test(l);

const libFiles = walk(at('lib'), n => n.endsWith('.ts'));
const specFiles = walk(at('tests'), n => n.endsWith('.spec.ts'));
const allTs = [...libFiles, ...walk(at('tests'), n => n.endsWith('.ts')), ...walk(at('fixtures'), n => n.endsWith('.ts'))];

const findings = {};
const add = (rule, f, n, detail) => (findings[rule] ??= []).push({file: rel(f), line: n, detail});

const ASYNC_MATCHER = /\.(toBeVisible|toBeHidden|toHaveText|toContainText|toHaveValue|toBeChecked|toBeEnabled|toBeDisabled|toHaveCount|toHaveAttribute|toBeAttached|toHaveClass|toBeEmpty|toBeFocused|toHaveCSS|toHaveURL|toHaveTitle|toBeInViewport|toBeEditable)\s*\(/;

for (const f of allTs) {
  const ls = read(f);
  // an `await expect(` / `await expect.poll(` can span lines; track whether we are inside one
  let openAwaitExpect = 0;
  ls.forEach((l, i) => {
    const n = i + 1;
    if (/\bawait\s+expect(\.\w+)?\s*\(/.test(l)) openAwaitExpect += (l.match(/\(/g) || []).length - (l.match(/\)/g) || []).length;
    else if (openAwaitExpect > 0) openAwaitExpect += (l.match(/\(/g) || []).length - (l.match(/\)/g) || []).length;
    if (openAwaitExpect < 0) openAwaitExpect = 0;
    const insideAwaitedExpect = openAwaitExpect > 0;

    if (isComment(l)) return;

    if (/forEach\s*\(\s*async/.test(l)) add('droppedPromise', f, n, 'forEach(async) discards the callback promise');
    if (ASYNC_MATCHER.test(l) && !/\b(await|return)\b/.test(l) && !/\.(then|catch)\b/.test(l) && !insideAwaitedExpect)
      add('droppedPromise', f, n, 'assertion is not awaited');

    // Only UNjustified escapes are counted. §3 permits a sleep or a force click when there is
    // genuinely no observable state, provided a comment says what it stands in for - so counting
    // justified ones would make the convention unfollowable: adding a properly justified sleep
    // would push the count over budget and the only way out would be raising the budget, which
    // §7 forbids. The comment is the contract; the budget tracks the ones that lack it.
    if (/waitForTimeout\s*\(/.test(l) && !/async waitForTimeout/.test(l) && !/waitForTimeout\((milliseconds|timeout)\)/.test(l) && !/\/\//.test(l))
      add('fixedSleep', f, n, 'sleep with no justification - say what it stands in for');

    if (/force:\s*true/.test(l) && !/\/\//.test(l))
      add('forceClick', f, n, 'force click with no justification - say which actionability check it overrides');

    if (/timeout:\s*\d{3,}/.test(l) && !/ConstantHelper/.test(l)) add('hardcodedTimeout', f, n, 'use ConstantHelper.timeout.*');

    if (/['"`]\/umbraco\/management\/api\/v1\//.test(l) && !/ConstantHelper\.ts$/.test(f) && !/ApiHelpers\.ts$/.test(f) && !/ApiHelper\.ts$/.test(f))
      add('hardcodedEndpoint', f, n, 'use ConstantHelper.apiEndpoints.*');
  });
}

// entity-name locators matched on a substring -> strict-mode multi-match on leftover data
const ENTITY_EL = /(uui-card-media|uui-card-block-type|uui-card-user|uui-table-row|umb-[a-z-]*-ref|umb-entity-item-ref|umb-tree-item|mediaCardItems|blockTypeCard|entityItem|listViewTableRow|elementCollectionViewTableRow|workspaceUserItemRefs|this\.results)/;
// The point of preferring this.click() is that it awaits visibility first, so what the rule has
// to detect is a click with no visibility wait - not the spelling of the call. Three shapes are
// therefore not counted, each of them already satisfying or unable to use the wrapper:
//
//   - the receiver was awaited visible in the preceding lines (`await expect(x).toBeVisible()`
//     or `await this.waitForVisible(x)` naming the same receiver), which is exactly what
//     this.click() would have done;
//   - the click passes an option this.click() cannot express - it takes only {force, timeout},
//     so a middle-click or a modifier click has no wrapper to use;
//   - it is a DOM click inside `evaluate(...)`, which is not a Playwright click at all.
//
// The previous version instead excluded any line matching `locator.click`, which exempted a call
// purely because its variable happened to be named `locator` - so BasePage.click's own click was
// exempt by coincidence while hoverAndClick's, three lines after its own toBeVisible, was not.
// Four of that rule's five findings were false positives.
const CLICK_OPTS_WRAPPER_LACKS = /\.click\s*\(\s*\{[^}]*\b(button|modifiers|position|clickCount|delay)\s*:/;
const receiverOf = line => {
  const at = line.indexOf('.click');
  if (at < 0) return null;
  // Walk back over the receiver expression, balancing brackets so a chained call survives.
  let depth = 0, start = at;
  for (let k = at - 1; k >= 0; k--) {
    const c = line[k];
    if (')]}'.includes(c)) depth++;
    else if ('([{'.includes(c)) { if (depth === 0) { start = k + 1; break; } depth--; }
    else if (depth === 0 && /[\s,;=]/.test(c)) { start = k + 1; break; }
    start = k;
  }
  return line.slice(start, at).trim();
};

for (const f of libFiles) {
  const ls = read(f);
  ls.forEach((l, i) => {
    if (isComment(l)) return;
    const m = l.match(/hasText:\s*([a-zA-Z_$][A-Za-z0-9_$]*)/);
    if (m && /Name$|^name$|Item$/.test(m[1]) && ENTITY_EL.test(l)) add('substringEntityName', f, i + 1, `{hasText: ${m[1]}} on an entity element`);
    // Literal only: `.nth(i)` with a variable index is the legitimate "the i-th block" form (§3).
    if (/\.nth\(\d+\)/.test(l)) add('literalIndexLocator', f, i + 1, 'hardcoded index');

    if (!/\.click\s*\(/.test(l)) return;
    if (/this\.(click|doubleClick|rightClick|javascriptClick|hoverAndClick)/.test(l)) return;
    if (/async (click|doubleClick|rightClick|javascriptClick)/.test(l)) return;
    if (/page\.mouse/.test(l)) return;
    if (/evaluate\s*\(/.test(l)) return;
    if (CLICK_OPTS_WRAPPER_LACKS.test(l)) return;

    const receiver = receiverOf(l);
    if (receiver) {
      const before = ls.slice(Math.max(0, i - 4), i).join('\n');
      const waited = new RegExp('(toBeVisible|waitForVisible)').test(before) && before.includes(receiver);
      if (waited) return;
    }
    add('rawClickInLib', f, i + 1, 'no visibility wait before this click - use this.click()');
  });
}

// builders are the typed boundary between specs and the Management API - an untyped build()
// or an `any` on the way to it lets a malformed payload compile and fail as an opaque 400.
//
// A return type only buys something where the returned shape is not already checked, so two
// shapes are deliberately NOT counted:
//   - a body that declares the value with an explicit payload type (`const values:
//     DataTypeValues = []`). That local annotation is what actually catches a misspelled
//     envelope field; a return type adds nothing, because an unannotated `const values = []`
//     infers `any[]` and `any[]` satisfies a `DataTypeValues` return annotation.
//   - a body that returns an empty literal, where there is no shape to get wrong.
// What is counted is an exit returning an inline object literal with no annotation. There the
// annotation does the work: TypeScript's excess-property check fires on a literal in a return
// position and names the mistake ("did you mean to write 'alias'?").
//
// Both halves were verified by planting a misspelled field and re-running tsc, because the
// earlier version of this rule counted the annotation rather than the effect and so reported
// 36 cases where adding one provably changes nothing.
const EXIT_SIG = /^  (abstract )?(build|getValues|getValue)\s*\([^)]*\)\s*\{?\s*$/;
for (const f of walk(at('lib/builders'), n => n.endsWith('.ts'))) {
  if (/types\.ts$/.test(f)) continue;
  const ls = read(f);
  ls.forEach((l, i) => {
    if (isComment(l)) return;
    if (/:\s*any\b/.test(l)) add('anyInBuilder', f, i + 1, 'an `any` here defeats the payload types downstream');
    if (!EXIT_SIG.test(l)) return;

    // Body by brace matching from the signature line.
    let depth = 0, body = [], started = false;
    for (let j = i; j < ls.length; j++) {
      for (const c of ls[j]) { if (c === '{') { depth++; started = true; } else if (c === '}') depth--; }
      if (j > i) body.push(ls[j]);
      if (started && depth <= 0) break;
    }
    const text = body.join('\n');

    // Nothing to get wrong.
    if (/^\s*return\s*(\[\s*\]|\{\s*\}|null|undefined)\s*;?\s*$/m.test(text) && !/return\s*\{\s*$/m.test(text)) return;

    // Already checked by an explicit, non-any annotation on the value that is returned.
    const declared = [...text.matchAll(/(?:const|let)\s+(\w+)\s*:\s*([A-Za-z][\w<>\[\]]*)\s*=/g)]
      .filter(m => m[2] !== 'any' && m[2] !== 'any[]');
    if (declared.some(m => new RegExp('return\\s+' + m[1] + '\\s*;').test(text))) return;

    // A pass-through to a shared builder utility - `return buildProperty({...})` - is checked at
    // the call site: excess-property checking applies to an argument literal just as it does to a
    // return literal, and BuilderUtils declares a return type on every one of these. Annotating
    // the exit as well would restate what the utility's signature already fixes.
    if (/return\s+[A-Za-z_$][\w$]*\s*\(/.test(text) && !/return\s*\{/.test(text)) return;

    add('untypedBuilderExit', f, i + 1, 'returns an unchecked shape - a return type would catch a misspelled field');
  });
}

// A helper parameter the body never reads is a signature promising something it does not do.
// The dangerous shape is a negation flag - an ignored `isVisible = true` means a caller passing
// `false` still gets a positive assertion, so the test asserts the opposite of what it reads.
const splitParams = s => {
  const out = []; let depth = 0, cur = '';
  for (const c of s) {
    if ('([{<'.includes(c)) depth++; else if (')]}>'.includes(c)) depth--;
    if (c === ',' && depth === 0) { out.push(cur); cur = ''; continue; }
    cur += c;
  }
  if (cur.trim()) out.push(cur);
  return out;
};
for (const f of walk(at('lib/helpers'), n => n.endsWith('.ts'))) {
  const ls = read(f);
  for (let i = 0; i < ls.length; i++) {
    const sigStart = ls[i].match(/^  (?:private |public )?(?:async )?([a-zA-Z][A-Za-z0-9]*)\s*\(/);
    // control flow at 2-space indent looks like a method signature but isn't
    const NOT_A_METHOD = ['constructor', 'if', 'for', 'while', 'switch', 'catch', 'return', 'do'];
    if (!sigStart || NOT_A_METHOD.includes(sigStart[1])) continue;
    let sig = '', d = 0, end = i;
    for (let j = i; j < ls.length; j++) {
      for (const c of ls[j]) { if (c === '(') d++; else if (c === ')') d--; }
      sig += ls[j] + ' '; end = j;
      if (d === 0) break;
    }
    const params = sig.slice(sig.indexOf('(') + 1, sig.lastIndexOf(')'));
    if (!params.trim()) continue;
    let body = '', bd = 0, started = false;
    for (let j = end; j < ls.length; j++) {
      for (const c of ls[j]) { if (c === '{') { bd++; started = true; } else if (c === '}') bd--; }
      if (j > end) body += ls[j] + '\n';
      if (started && bd === 0) break;
    }
    if (!started) continue;
    // Strip member accesses first: a parameter named `isVisible` would otherwise look used by
    // the `this.isVisible(...)` call in its own body - which is precisely the flag shape this
    // rule exists to catch, so without this the rule is blind to its main target.
    // The lookbehind keeps spread syntax intact: without it, `{...extraHeaders}` is read as a
    // member access and a genuinely-used parameter is reported as unused.
    const bodyIdentifiers = body.replace(/(?<!\.)\.\s*[A-Za-z_$][A-Za-z0-9_$]*/g, '.');
    for (const raw of splitParams(params)) {
      const pm = raw.trim().match(/^([a-zA-Z_$][A-Za-z0-9_$]*)\s*[?:=]/) || raw.trim().match(/^([a-zA-Z_$][A-Za-z0-9_$]*)$/);
      if (!pm || new RegExp(`\\b${pm[1]}\\b`).test(bodyIdentifiers)) continue;
      const isFlag = /boolean|=\s*(true|false)/.test(raw);
      add('unusedHelperParam', f, i + 1, `${sigStart[1]}() never reads '${pm[1]}'` + (isFlag ? ' - an ignored flag inverts the assertion' : ''));
    }
  }
}

// check-method contract: a does*/is*/has* that only returns a value is a silent green when bare-awaited
const PRIMITIVE = /\b(isVisible|hasText|containsText|hasCount|hasValue|hasAttribute|isEnabled|isDisabled|doesNotContainText|waitForVisible|waitForHidden|waitForText|waitForValue)\s*\(|expect\s*\(/;
const methods = new Map();
for (const f of libFiles) {
  const ls = read(f);
  for (let i = 0; i < ls.length; i++) {
    const m = ls[i].match(/^  (?:private |public )?(?:async )?([a-zA-Z][A-Za-z0-9]*)\s*\(/);
    if (!m) continue;
    let body = '';
    for (let j = i + 1; j < ls.length && !/^  \}/.test(ls[j]); j++) body += ls[j] + '\n';
    if (!methods.has(m[1])) methods.set(m[1], {body, file: f, line: i + 1});
  }
}
const asserts = new Set([...methods].filter(([, v]) => PRIMITIVE.test(v.body)).map(([k]) => k));
for (let changed = true; changed;) {
  changed = false;
  for (const [n, {body}] of methods) {
    if (asserts.has(n)) continue;
    for (const c of asserts) if (body.includes(`this.${c}(`)) { asserts.add(n); changed = true; break; }
  }
}
const pureChecks = [...methods.keys()].filter(n => /^(does|is|has|verify)[A-Z]/.test(n) && !asserts.has(n) && /^\s*return /m.test(methods.get(n).body));
for (const f of specFiles) read(f).forEach((l, i) => {
  if (isComment(l)) return;
  const m = l.match(/await\s+umbraco(?:Ui|Api)\.[A-Za-z]+\.((?:does|is|has|verify)[A-Z][A-Za-z0-9]*)\s*\(/);
  if (m && !/expect\s*\(/.test(l) && pureChecks.includes(m[1]))
    add('discardedCheck', f, i + 1, `${m[1]}() returns a value but nothing asserts on it`);

  // The third way to lose an assertion, and the quietest: wrapping an un-awaited async helper
  // in expect(). `expect(umbracoUi.user.isUserDisabledTextVisible()).toBeTruthy()` asserts that
  // a Promise is truthy - which it always is - so the assertion can never fail. Note this must
  // NOT fire on the synchronous getters (getPropertyValue, getOnlyPropertyValue), where wrapping
  // in expect() is exactly right.
  const wrapped = l.match(/expect\(\s*umbraco(?:Ui|Api)\.[A-Za-z]+\.((?:does|is|has|verify)[A-Z][A-Za-z0-9]*)\s*\(/);
  if (wrapped && !/expect\(\s*await\b/.test(l) && asserts.has(wrapped[1]))
    add('promiseInExpect', f, i + 1, `${wrapped[1]}() is async - expect() on the promise is always true`);

  // The complement, and it fails silently: a helper that asserts internally, called at
  // statement position WITHOUT await. Its assertions still run, but a failure becomes a
  // rejected promise nobody handles - Playwright reports "1 passed" plus an error that is
  // "not a part of any test". A real failure reads as a green test.
  const bare = l.match(/^\s*umbraco(?:Ui|Api)\.[A-Za-z]+\.((?:does|is|has|verify)[A-Z][A-Za-z0-9]*)\s*\(/);
  if (bare && asserts.has(bare[1]))
    add('unawaitedAssertion', f, i + 1, `${bare[1]}() asserts internally - without await a failure passes as green`);
});

// A spec asserting on a raw API response reaches past the helper layer the same way a raw
// `page.locator` does: it hard-codes the response shape into 269 files, gives a failure
// message with no entity name in it, and (with `values[0]`) often assumes an order the API
// does not promise. Prefer an assertion helper - see ApiHelpers.doesPropertyHaveValue.
for (const f of specFiles) {
  const ls = read(f);
  // Key on provenance, not on the name: a `*Data` variable built locally (say, returned from a
  // UI helper) is not an API response shape, and flagging it on the name alone is a false
  // positive. Only variables fetched from umbracoApi count.
  const fromApi = new Set();
  for (const l of ls) {
    const m = l.match(/(?:const|let)\s+([a-zA-Z][A-Za-z0-9]*)\s*=\s*await\s+umbracoApi\./);
    if (m) fromApi.add(m[1]);
  }
  ls.forEach((l, i) => {
    if (isComment(l)) return;
    // Requires a member access after the variable: `expect(contentData).toBeTruthy()` only checks
    // that the fetch returned something and couples to no response shape, so it is not a finding.
    const m = l.match(/^\s*expect\(([a-zA-Z]+(?:Data|Json))[.[][^)]*\)\.(?:toBe|toEqual|toBeTruthy|toBeFalsy|toContain|toHaveLength)/);
    if (m && fromApi.has(m[1]))
      add('rawResponseAssertion', f, i + 1, 'assert through an api helper instead of the raw response shape');
  });
}

// disabled tests must carry a report-visible annotation, not just a comment
for (const f of specFiles) {
  const ls = read(f);
  ls.forEach((l, i) => {
    if (!/^\s*test\.(skip|fixme)\s*\(/.test(l)) return;
    const window = ls.slice(i, i + 3).join(' ');
    if (!/annotation\s*:/.test(window)) add('disabledWithoutAnnotation', f, i + 1, 'add {annotation: {type, description}} so it surfaces in reports');
  });
}

// Comment policy (root CLAUDE.md §9). A commented-out test is the most hidden form of
// disabled test: invisible to `--list`, to the reporters, and to the annotation rule above.
// A commented-out assertion is worse than a deleted one - the test still passes while
// asserting less than it appears to. A TODO with no author or version trigger cannot rot
// out loud, so it never gets removed.
for (const f of specFiles) {
  const ls = read(f);
  // A file with no live test is wholly dead; its commented assertions belong to that finding,
  // not to commentedOutAssertion, which is about a *live* test asserting less than it looks like.
  const hasLiveTest = ls.some(l => /^\s*test[.(]/.test(l));
  ls.forEach((l, i) => {
    if (/^\s*\/\/\s*test[.(]/.test(l))
      add('commentedOutTest', f, i + 1, 'use test.skip with an annotation so it is visible in reports');
    else if (hasLiveTest && /^\s*\/\/\s*(await|expect)\b/.test(l))
      add('commentedOutAssertion', f, i + 1, 'restore it or delete it - a commented assertion silently weakens the test');
  });
}
for (const f of allTs) {
  read(f).forEach((l, i) => {
    const m = l.match(/\/\/\s*TODO\b(.*)$/i);
    // anchored = carries a version trigger (V19) or an author initial tag ([NL])
    if (m && !/\(V\d+\)|\[[A-Z]{2,3}\]|https?:\/\//.test(m[0]))
      add('unanchoredTodo', f, i + 1, 'anchor it: // TODO (V19): ... or // TODO: ... [XX]');
  });
}

// lib/ is a published, semver'd package. A deprecation with no removal version tells a
// consumer nothing about their runway, and tells us nothing about when deletion is safe.
for (const f of libFiles) {
  const ls = read(f);
  ls.forEach((l, i) => {
    if (!/@deprecated/.test(l)) return;
    // the removal note may sit on the same line or in the following JSDoc lines
    const block = ls.slice(i, i + 5).join(' ');
    if (!/Scheduled for removal in \d+\.\d+/.test(block))
      add('deprecationWithoutRemoval', f, i + 1, 'add "Scheduled for removal in <major+2>.0."');
  });
}

// ConstantHelper.apiEndpoints hardcodes ~46 Management API paths. Nothing otherwise ties them
// to the API's own contract, so a renamed route surfaces as a helper waiting for a response
// that never arrives - a mystifying timeout rather than "this endpoint is gone". The committed
// OpenApi.json is the contract and needs no running instance to read.
const OPENAPI = path.join(ROOT, '..', '..', 'src', 'Umbraco.Cms.Api.Management', 'OpenApi.json');
// Auth endpoints are served by OpenIddict middleware rather than MVC controllers, so they are
// absent from the generated document by design.
const NOT_IN_OPENAPI = [
  '/umbraco/management/api/v1/security/back-office/revoke',
  '/umbraco/management/api/v1/security/back-office/token',
  '/umbraco/management/api/v1/security/back-office/login',
];
if (fs.existsSync(OPENAPI)) {
  let specPaths = null;
  try {
    specPaths = Object.keys(JSON.parse(fs.readFileSync(OPENAPI, 'utf8')).paths || {});
  } catch {
    specPaths = null;   // a malformed document is the OpenAPI sync's problem, not this audit's
  }
  const known = p => specPaths.some(sp => sp === p || sp.startsWith(p + '/') || sp.startsWith(p + '{'));

  const constFile = path.join(ROOT, 'lib', 'helpers', 'ConstantHelper.ts');
  if (specPaths && specPaths.length && fs.existsSync(constFile)) {
    const src = fs.readFileSync(constFile, 'utf8');
    const from = src.indexOf('apiEndpoints = {');
    const block = from === -1 ? '' : src.slice(from, src.indexOf('}', from));
    const lineOf = name => src.slice(0, src.indexOf(`${name}:`, from)).split(/\r?\n/).length;
    for (const m of block.matchAll(/(\w+):\s*'([^']+)'/g)) {
      const [, name, p] = m;
      if (!p.startsWith('/umbraco/management/api/v1/') || NOT_IN_OPENAPI.includes(p)) continue;
      if (!known(p)) add('endpointNotInOpenApi', constFile, lineOf(name), `apiEndpoints.${name} -> ${p} is not in OpenApi.json`);
    }
  }

  // 232 paths are written inline in the *ApiHelper files rather than as constants, so checking
  // only ConstantHelper would leave the vast majority of the suite's API surface unverified.
  // Style aside, what matters is that the route still exists.
  if (specPaths && specPaths.length) {
    for (const f of walk(at('lib/helpers'), n => /ApiHelper(s)?\.ts$/.test(n))) {
      read(f).forEach((l, i) => {
        if (isComment(l)) return;
        for (const m of l.matchAll(/['"`](\/umbraco\/management\/api\/v1\/[A-Za-z0-9/-]+)/g)) {
          // trim a trailing slash left by string concatenation (".../document/' + id")
          const p = m[1].replace(/\/$/, '');
          if (NOT_IN_OPENAPI.includes(p)) continue;
          if (!known(p)) add('endpointNotInOpenApi', f, i + 1, `${p} is not in OpenApi.json`);
        }
      });
    }
  }
}

// Leftover data is the documented root cause of the suite's strict-mode failures, and the suite
// is serial - so whatever a spec leaves behind becomes the next spec's problem. Flag a spec that
// creates an entity type it never tears down.
//
// Deleting a type cascades to its instances, so an entity whose *type* is cleaned needs no
// teardown of its own; only entities with no cascade must be removed explicitly.
const CASCADES_FROM = {
  document: ['documentType'],
  documentBlueprint: ['documentType'],
  element: ['documentType'],
  media: ['mediaType'],
  member: ['memberType'],
};
const NEEDS_TEARDOWN = new Set([
  'document', 'documentType', 'documentBlueprint', 'dataType', 'media', 'mediaType',
  'member', 'memberType', 'memberGroup', 'language', 'dictionary', 'template',
  'partialView', 'stylesheet', 'script', 'user', 'userGroup', 'webhook', 'relationType',
  'element',
]);
for (const f of specFiles) {
  const ls = read(f);
  const created = new Map();
  const cleaned = new Set();
  ls.forEach((l, i) => {
    if (isComment(l)) return;
    const c = l.match(/umbracoApi\.([a-zA-Z]+)\.(?:create|save|import|copy|duplicate)[A-Za-z]*\s*\(/);
    if (c && NEEDS_TEARDOWN.has(c[1]) && !created.has(c[1])) created.set(c[1], i + 1);
    const d = l.match(/umbracoApi\.([a-zA-Z]+)\.(?:ensureNameNotExists|ensureIsoCodeNotExists|ensureEmailNotExists|delete)/);
    if (d) cleaned.add(d[1]);
  });
  for (const [entity, line] of created) {
    if (cleaned.has(entity)) continue;
    if ((CASCADES_FROM[entity] || []).some(parent => cleaned.has(parent))) continue;
    add('entityNotTornDown', f, line, `creates ${entity} but nothing in this file removes it`);
  }
}

// a spec outside a playwright project directory is silently never run
const PROJECT_DIRS = ['DefaultConfig', 'ExtensionRegistry', 'EntityDataPicker', 'DeliveryApi', 'ContentSettingConfig', 'SMTP', 'ImagingSettingConfig', 'ExternalLogin', 'AuthProviderLateRegistration', 'UnattendedInstallConfig'];
for (const f of specFiles) {
  const seg = rel(f).split('/')[1];
  if (!PROJECT_DIRS.includes(seg)) add('orphanSpec', f, 1, 'no playwright project testMatch claims this file');
}

// Specs go through page objects. Any reach for `page` counts, not just a chosen list of its
// methods: the earlier version listed locator|getBy|click|goto|fill|waitFor, which left
// `umbracoUi.page.url()`, `.reload()`, `.keyboard` and `.evaluate()` uncounted - six sites, two of
// them on the lines either side of one it did flag.
for (const f of specFiles) read(f).forEach((l, i) => {
  if (isComment(l)) return;
  if (/(^|[^.\w])page\.[a-zA-Z]/.test(l)) add('rawFixtureInSpec', f, i + 1, 'use a page object, not the raw page fixture');
  else if (/umbraco(Ui|Api)\.page\.[a-zA-Z]/.test(l)) add('helperPageInSpec', f, i + 1, 'wrap this in a helper method');
});

// ---- report ----------------------------------------------------------------
const RULES = [
  ['droppedPromise', 'Dropped promises (assertion never runs)'],
  ['discardedCheck', 'Value-returning check with nothing asserting on it'],
  ['unawaitedAssertion', 'Assert-internally helper called without await'],
  ['endpointNotInOpenApi', 'Endpoint constant absent from OpenApi.json'],
  ['entityNotTornDown', 'Spec creates an entity it never tears down'],
  ['promiseInExpect', 'expect() on an un-awaited async helper (always true)'],
  ['orphanSpec', 'Spec outside a playwright project directory'],
  ['rawFixtureInSpec', 'Raw page fixture used in a spec'],
  ['hardcodedEndpoint', 'Hardcoded API endpoint'],
  ['disabledWithoutAnnotation', 'Disabled test without an annotation'],
  ['fixedSleep', 'Fixed sleep (waitForTimeout)'],
  ['forceClick', 'force: true click'],
  ['substringEntityName', 'Entity name matched on a substring'],
  ['rawClickInLib', 'Raw .click() in lib'],
  ['literalIndexLocator', 'Hardcoded .nth(N) index'],
  ['hardcodedTimeout', 'Hardcoded timeout in ms'],
  ['helperPageInSpec', 'Spec reaching through a helper to page'],
  ['commentedOutTest', 'Commented-out test (invisible to every report)'],
  ['commentedOutAssertion', 'Commented-out assertion in a live test'],
  ['untypedBuilderExit', 'Builder exit returning an unchecked shape'],
  ['anyInBuilder', '`: any` inside a builder'],
  ['unanchoredTodo', 'TODO with no version trigger or author'],
  ['rawResponseAssertion', 'Spec asserting on a raw API response shape'],
  ['deprecationWithoutRemoval', 'Deprecation with no removal version'],
  ['unusedHelperParam', 'Helper parameter the body never reads'],
];

const verbose = process.argv.includes('--verbose');
let failed = 0;
console.log(`\nConvention audit — ${specFiles.length} specs, ${libFiles.length} lib files\n`);
console.log('  ' + 'rule'.padEnd(52) + 'found'.padStart(6) + 'budget'.padStart(8) + '   status');
console.log('  ' + '-'.repeat(78));
for (const [key, label] of RULES) {
  const found = (findings[key] || []).length;
  const budget = BUDGET[key] ?? 0;
  const over = found > budget;
  if (over) failed++;
  const status = over ? 'OVER BUDGET' : (found === 0 ? 'clean' : (found < budget ? 'improved' : 'at budget'));
  console.log('  ' + label.padEnd(52) + String(found).padStart(6) + String(budget).padStart(8) + '   ' + status);
  if (!over && !verbose) continue;
  const list = findings[key] || [];
  // when a budgeted rule goes over, the pre-existing entries are noise - cap the dump
  const shown = verbose ? list : list.slice(0, Math.max(5, found - budget) + 5);
  for (const x of shown) console.log(`        ${x.file}:${x.line}  ${x.detail}`);
  if (shown.length < list.length) console.log(`        ... and ${list.length - shown.length} more (--verbose for all)`);
}

const improved = RULES.filter(([k]) => (findings[k] || []).length < (BUDGET[k] ?? 0));
if (improved.length) {
  console.log('\n  Debt went down — lower these budgets in audit-conventions.js:');
  for (const [k, label] of improved) console.log(`    ${k}: ${(findings[k] || []).length}   (${label})`);
}

if (failed) {
  console.log(`\n${failed} rule(s) over budget. See CLAUDE.md §3 for what each rule protects.\n`);
  process.exit(1);
}
console.log('\nAll rules within budget.\n');
