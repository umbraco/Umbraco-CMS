import { toCamelCase } from './to-camel-case.function.js';
import { expect } from '@open-wc/testing';

describe('to-camel-case', () => {
	/* All tests have been run against the the lodash camelCase function, 
  to ensure that the toCamelCase function behaves the same way as the lodash camelCase function. */
	it('converts string to camelCase', () => {
		expect(toCamelCase('to-camel-case')).to.equal('toCamelCase');
		expect(toCamelCase('b2b_registration_request')).to.equal('b2BRegistrationRequest');
		expect(toCamelCase('b2b_registration_b2b_request')).to.equal('b2BRegistrationB2BRequest');
		expect(toCamelCase('foo')).to.equal('foo');
		expect(toCamelCase('IDs')).to.equal('iDs');
		expect(toCamelCase('FooIDs')).to.equal('fooIDs');
		expect(toCamelCase('foo-bar')).to.equal('fooBar');
		expect(toCamelCase('foo-bar-baz')).to.equal('fooBarBaz');
		expect(toCamelCase('foo--bar')).to.equal('fooBar');
		expect(toCamelCase('--foo-bar')).to.equal('fooBar');
		expect(toCamelCase('FOO-BAR')).to.equal('fooBar');
		expect(toCamelCase('-foo-bar-')).to.equal('fooBar');
		expect(toCamelCase('--foo--bar--')).to.equal('fooBar');
		expect(toCamelCase('foo.bar')).to.equal('fooBar');
		expect(toCamelCase('foo..bar')).to.equal('fooBar');
		expect(toCamelCase('..foo..bar..')).to.equal('fooBar');
		expect(toCamelCase('foo_bar')).to.equal('fooBar');
		expect(toCamelCase('__foo__bar__')).to.equal('fooBar');
		expect(toCamelCase('foo bar')).to.equal('fooBar');
		expect(toCamelCase('  foo  bar  ')).to.equal('fooBar');
		expect(toCamelCase('fooBar')).to.equal('fooBar');
		expect(toCamelCase('fooBar-baz')).to.equal('fooBarBaz');
		expect(toCamelCase('fooBarBaz-bazzy')).to.equal('fooBarBazBazzy');
		expect(toCamelCase('FBBazzy')).to.equal('fbBazzy');
		expect(toCamelCase('F')).to.equal('f');
		expect(toCamelCase('Foo')).to.equal('foo');
		expect(toCamelCase('FOO')).to.equal('foo');
		expect(toCamelCase('FooBar')).to.equal('fooBar');
		expect(toCamelCase('Foo')).to.equal('foo');
		expect(toCamelCase('FOO')).to.equal('foo');
		expect(toCamelCase('XMLHttpRequest')).to.equal('xmlHttpRequest');
		expect(toCamelCase('AjaxXMLHttpRequest')).to.equal('ajaxXmlHttpRequest');
		expect(toCamelCase('Ajax-XMLHttpRequest')).to.equal('ajaxXmlHttpRequest');
		expect(toCamelCase('mGridCol6')).to.equal('mGridCol6');
		expect(toCamelCase('Hello1World')).to.equal('hello1World');
		expect(toCamelCase('Hello11World')).to.equal('hello11World');
		expect(toCamelCase('hello1world')).to.equal('hello1World');
		expect(toCamelCase('Hello1World11foo')).to.equal('hello1World11Foo');
		expect(toCamelCase('Hello1')).to.equal('hello1');
		expect(toCamelCase('hello1')).to.equal('hello1');
		expect(toCamelCase('1Hello')).to.equal('1Hello');
		expect(toCamelCase('1hello')).to.equal('1Hello');
		expect(toCamelCase('1hello')).to.equal('1Hello');
		expect(toCamelCase('h2w')).to.equal('h2W');
	});

	it('ignores special characters', () => {
		expect(toCamelCase('-')).to.equal('');
		expect(toCamelCase(' - ')).to.equal('');
		expect(toCamelCase('--')).to.equal('');
		expect(toCamelCase('')).to.equal('');
		expect(toCamelCase(' ')).to.equal('');
		expect(toCamelCase('_')).to.equal('');
		expect(toCamelCase('.')).to.equal('');
		expect(toCamelCase('..')).to.equal('');
		expect(toCamelCase('  ')).to.equal('');
		expect(toCamelCase('__')).to.equal('');
		expect(toCamelCase('--__--_--_')).to.equal('');
		expect(toCamelCase('A::a')).to.equal('aA');
		expect(toCamelCase('foo bar?')).to.equal('fooBar');
		expect(toCamelCase('foo bar!')).to.equal('fooBar');
		expect(toCamelCase('foo bar$')).to.equal('fooBar');
		expect(toCamelCase('foo bar#')).to.equal('fooBar');
	});
});
