import { expect } from '@open-wc/testing';
import { getTextFromDescendants } from './get-text-from-descendants.function.js';

describe('getTextFromDescendants', () => {
	it('returns an empty string for null input', () => {
		expect(getTextFromDescendants(null)).to.equal('');
	});

	it('returns an empty string for undefined input', () => {
		expect(getTextFromDescendants(undefined)).to.equal('');
	});

	it('reads text from a plain element with no children', () => {
		const element = document.createElement('div');
		element.textContent = 'Hello world';
		expect(getTextFromDescendants(element)).to.equal('Hello world');
	});

	it('concatenates text from nested light-DOM children', () => {
		const element = document.createElement('div');
		element.innerHTML = 'Hello <strong>brave</strong> <em>new world</em>';
		expect(getTextFromDescendants(element)).to.equal('Hello brave new world');
	});

	it('returns an empty string for an element with no text descendants', () => {
		const element = document.createElement('div');
		element.appendChild(document.createElement('img'));
		expect(getTextFromDescendants(element)).to.equal('');
	});

	it('reads text from an element that has a shadow root', () => {
		const element = document.createElement('div');
		const shadow = element.attachShadow({ mode: 'open' });
		shadow.textContent = 'Inside shadow';
		expect(getTextFromDescendants(element)).to.equal('Inside shadow');
	});

	it('prefers the shadow root over light DOM children when both exist', () => {
		const element = document.createElement('div');
		element.textContent = 'Light text';
		const shadow = element.attachShadow({ mode: 'open' });
		shadow.textContent = 'Shadow text';
		expect(getTextFromDescendants(element)).to.equal('Shadow text');
	});

	it('descends into nested shadow roots', () => {
		const outer = document.createElement('div');
		const outerShadow = outer.attachShadow({ mode: 'open' });
		const inner = document.createElement('span');
		const innerShadow = inner.attachShadow({ mode: 'open' });
		innerShadow.textContent = 'Resolved name';
		outerShadow.appendChild(inner);
		expect(getTextFromDescendants(outer)).to.equal('Resolved name');
	});

	it('accepts a ShadowRoot directly as input', () => {
		const host = document.createElement('div');
		const shadow = host.attachShadow({ mode: 'open' });
		shadow.textContent = 'Direct shadow';
		expect(getTextFromDescendants(shadow)).to.equal('Direct shadow');
	});
});
