/**
 * Self-test for the API assertion helpers on ApiHelpers.
 *
 * These helpers back ~700 spec assertions, and their contract has a subtlety that is easy to
 * get wrong: a property that varies by culture or segment contributes **one `values` entry per
 * culture/segment**, so a single-property document can legitimately carry several entries. The
 * first version of `getOnlyPropertyValue` asserted `values.length === 1` and would have failed
 * every variant and segment spec it was meant to serve. The cases below pin that down.
 *
 * Runs against fabricated response objects — no Umbraco instance, no browser.
 *
 * Note: the `message` argument to `expect()` is rendered by the Playwright reporter and is not
 * stored on the thrown error, so standalone these cases assert *that* a helper rejects, not what
 * it says while rejecting.
 *
 * Run: npm run helpers:selftest
 */
const path = require('path');
const {expect} = require('@playwright/test');

// The helpers never touch this.page, so a bare instance is enough to exercise them.
const {ApiHelpers} = require(path.join(__dirname, 'dist', 'helpers', 'ApiHelpers.js'));
const api = new ApiHelpers(null);

let passed = 0, failed = 0;

async function ok(name, fn) {
  try {
    await fn();
    console.log(`  ok    ${name}`);
    passed++;
  } catch (e) {
    console.error(`  FAIL  ${name}\n          ${String(e.message).split('\n')[0]}`);
    failed++;
  }
}

async function rejects(name, fn) {
  try {
    await fn();
    console.error(`  FAIL  ${name}\n          expected an assertion failure, got none`);
    failed++;
  } catch {
    console.log(`  ok    ${name} (rejected)`);
    passed++;
  }
}

// --- fixtures ---------------------------------------------------------------
const invariant = {
  name: 'Doc',
  values: [{alias: 'title', value: 'hello', culture: null, segment: null}],
  variants: [{name: 'Doc', culture: null, state: 'Published'}],
};

// ONE property varying across two cultures -> two entries. The case that was broken.
const varying = {
  name: 'Doc',
  values: [
    {alias: 'title', value: 'hello', culture: 'en-US', segment: null},
    {alias: 'title', value: 'hej', culture: 'da', segment: null},
  ],
  variants: [
    {name: 'Doc', culture: 'en-US', state: 'Published'},
    {name: 'Dok', culture: 'da', state: 'Draft'},
  ],
};

// ONE property across two segments -> two entries.
const segmented = {
  name: 'Doc',
  values: [
    {alias: 'title', value: 'default', culture: null, segment: null},
    {alias: 'title', value: 'seg', culture: null, segment: 'alpha'},
  ],
  variants: [{name: 'Doc', culture: null, state: 'Published'}],
};

const twoProps = {
  name: 'Doc',
  values: [
    {alias: 'title', value: 'a', culture: null, segment: null},
    {alias: 'body', value: 'b', culture: null, segment: null},
  ],
  variants: [{name: 'Doc', culture: null, state: 'Published'}],
};

const contentType = {
  name: 'DT',
  isElement: true,
  properties: [{alias: 'title', dataType: {id: 'dt-1'}}],
  compositions: [{documentType: {id: 'c-1'}}],
};

// Two definitions, deliberately not in alias order, so a lookup that works cannot be relying
// on position. `body` also carries the nested shapes the specs assert on.
const twoDefinitions = {
  name: 'DT2',
  properties: [
    {alias: 'title', name: 'Title', dataType: {id: 'dt-1'}},
    {
      alias: 'body',
      name: 'Body',
      description: 'the body',
      dataType: {id: 'dt-2'},
      variesByCulture: true,
      validation: {mandatory: true, regEx: '^a', regExMessage: 'nope'},
      appearance: {labelOnTop: true},
    },
  ],
};

(async () => {
  console.log('\ngetOnlyPropertyValue');
  await ok('single invariant property', () => expect(api.getOnlyPropertyValue(invariant)).toBe('hello'));
  await ok('one property varying by culture (two entries)', () => expect(api.getOnlyPropertyValue(varying)).toBe('hello'));
  await ok('one property varying by segment (two entries)', () => expect(api.getOnlyPropertyValue(segmented)).toBe('default'));
  await rejects('two distinct properties', () => api.getOnlyPropertyValue(twoProps));
  await rejects('missing values array', () => api.getOnlyPropertyValue({}));

  console.log('\ngetPropertyValue');
  await ok('finds by alias regardless of position', () => expect(api.getPropertyValue(twoProps, 'body')).toBe('b'));
  await ok('varying property, no culture -> first entry', () => expect(api.getPropertyValue(varying, 'title')).toBe('hello'));
  await ok('targets a culture when given', () => expect(api.getPropertyValue(varying, 'title', 'da')).toBe('hej'));
  await rejects('absent alias', () => api.getPropertyValue(invariant, 'nope'));
  await rejects('absent culture', () => api.getPropertyValue(varying, 'title', 'de'));

  console.log('\ndoesPropertyHaveValue');
  await ok('matching invariant value', () => api.doesPropertyHaveValue(invariant, 'title', 'hello'));
  await ok('varying property without a culture', () => api.doesPropertyHaveValue(varying, 'title', 'hello'));
  await ok('targeted culture', () => api.doesPropertyHaveValue(varying, 'title', 'hej', 'da'));
  await rejects('wrong value', () => api.doesPropertyHaveValue(invariant, 'title', 'wrong'));
  await rejects('absent alias', () => api.doesPropertyHaveValue(invariant, 'nope', 'x'));

  console.log('\ndoesVariantHaveState / Name / Count');
  await ok('default variant state', () => api.doesVariantHaveState(invariant, 'Published'));
  await ok('default variant is the first, as values[0] was', () => api.doesVariantHaveState(varying, 'Published'));
  await ok('targets a culture for state', () => api.doesVariantHaveState(varying, 'Draft', 'da'));
  await ok('default variant name', () => api.doesVariantHaveName(invariant, 'Doc'));
  await ok('targets a culture for name', () => api.doesVariantHaveName(varying, 'Dok', 'da'));
  await ok('variant count', () => api.doesHaveVariantCount(varying, 2));
  await rejects('wrong state', () => api.doesVariantHaveState(invariant, 'Draft'));
  await rejects('unknown culture', () => api.doesVariantHaveState(varying, 'Published', 'de'));
  await rejects('wrong variant count', () => api.doesHaveVariantCount(varying, 1));

  console.log('\ndoesHaveValueCount');
  await ok('counts entries', () => api.doesHaveValueCount(twoProps, 2));
  await ok('zero means nothing set', () => api.doesHaveValueCount({values: []}, 0));
  await rejects('wrong count', () => api.doesHaveValueCount(twoProps, 1));

  console.log('\ngetOnlyPropertyDefinition / getPropertyDefinition');
  await ok('the sole definition', () => expect(api.getOnlyPropertyDefinition(contentType).alias).toBe('title'));
  await rejects('two definitions is not "only"', () => api.getOnlyPropertyDefinition(twoDefinitions));
  await rejects('missing properties array', () => api.getOnlyPropertyDefinition({}));
  await ok('finds by alias regardless of position', () => expect(api.getPropertyDefinition(twoDefinitions, 'body').name).toBe('Body'));
  await ok('reaches the nested validation shape', () => expect(api.getPropertyDefinition(twoDefinitions, 'body').validation.regEx).toBe('^a'));
  await ok('reaches the nested appearance shape', () => expect(api.getPropertyDefinition(twoDefinitions, 'body').appearance.labelOnTop).toBe(true));
  await rejects('absent alias', () => api.getPropertyDefinition(twoDefinitions, 'nope'));
  // The counting rule is the opposite of getOnlyPropertyValue's on purpose: a definition is one
  // entry per property, so length is the count - whereas a *value* is one entry per culture.
  await ok('a culture-varying definition is still one entry', () => expect(api.getPropertyDefinition(twoDefinitions, 'body').variesByCulture).toBe(true));

  console.log('\ncontent type helpers');
  await ok('property definition count', () => api.doesHavePropertyCount(contentType, 1));
  await ok('composition count', () => api.doesHaveCompositionCount(contentType, 1));
  await ok('property uses data type', () => api.doesPropertyUseDataType(contentType, 'title', 'dt-1'));
  await ok('only property uses data type', () => api.doesOnlyPropertyUseDataType(contentType, 'dt-1'));
  await ok('isElementType true', () => api.isElementType(contentType));
  await ok('isElementType false', () => api.isElementType({name: 'X', isElement: false}, false));
  await rejects('wrong data type', () => api.doesOnlyPropertyUseDataType(contentType, 'dt-2'));
  await rejects('"only" with two properties', () => api.doesOnlyPropertyUseDataType(
    {properties: [{alias: 'a', dataType: {id: '1'}}, {alias: 'b', dataType: {id: '2'}}]}, '1'));
  await rejects('absent property alias', () => api.doesPropertyUseDataType(contentType, 'nope', 'dt-1'));

  console.log('\ndoesHaveContent');
  await ok('matching content', () => api.doesHaveContent({name: 'T', content: 'abc'}, 'abc'));
  await rejects('mismatched content', () => api.doesHaveContent({name: 'T', content: 'abc'}, 'xyz'));

  // --- builders -------------------------------------------------------------
  // A setter whose value never reaches the payload is a silent no-op. The entityType output
  // was keyed on `editorAlias`, so withEntityType() was discarded on every call.
  console.log('\nbuilder payloads');
  const {MediaBuilder, DocumentBuilder} = require(path.join(__dirname, 'dist', 'builders', 'index.js'));

  const withExplicit = new MediaBuilder().withMediaTypeId('mt')
    .addValue().withAlias('title').withEntityType('media-property-value').withValue('v').done()
    .build();
  await ok('withEntityType() reaches the payload',
    () => expect(withExplicit.values[0].entityType).toBe('media-property-value'));

  const withEditorAlias = new MediaBuilder().withMediaTypeId('mt')
    .addValue().withAlias('title').withEditorAlias('Umbraco.TextBox').withValue('v').done()
    .build();
  await ok('the editorAlias-keyed default is unchanged',
    () => expect(withEditorAlias.values[0].entityType).toBe('media-property-value'));

  const withNeither = new MediaBuilder().withMediaTypeId('mt')
    .addValue().withAlias('title').withValue('v').done()
    .build();
  await ok('neither set still yields null',
    () => expect(withNeither.values[0].entityType).toBeNull());

  const doc = new DocumentBuilder().withDocumentTypeId('dt')
    .addValue().withAlias('title').withEntityType('document-property-value').withValue('v').done()
    .build();
  await ok('same contract on DocumentValueBuilder',
    () => expect(doc.values[0].entityType).toBe('document-property-value'));

  // Client-side id generation is NOT uniform: 13 builders call ensureIdExists (document,
  // element, member, the content types, dataType...), while media, userGroup and user emit
  // null and let the server assign. Pinned here so a future change to either is deliberate.
  await ok('DocumentBuilder generates an id client-side',
    () => expect(typeof new DocumentBuilder().withDocumentTypeId('dt').build().id).toBe('string'));
  await ok('MediaBuilder leaves the id for the server',
    () => expect(new MediaBuilder().withMediaTypeId('mt').build().id).toBeNull());
  await ok('an explicit withId() wins on both',
    () => expect(new MediaBuilder().withId('fixed').withMediaTypeId('mt').build().id).toBe('fixed'));

  console.log(`\n${passed + failed} case(s), ${failed} failure(s)`);
  process.exit(failed ? 1 : 0);
})();
