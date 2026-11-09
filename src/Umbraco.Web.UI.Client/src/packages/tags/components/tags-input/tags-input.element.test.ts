import { UmbTagsInputElement } from './tags-input.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbTagsInputElement', () => {
	let element: UmbTagsInputElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-tags-input></umb-tags-input>`);
	});

	function getInput(): HTMLInputElement {
		return element.shadowRoot!.querySelector('#tag-input') as HTMLInputElement;
	}

	function dispatchPaste(text: string): ClipboardEvent {
		const clipboardData = new DataTransfer();
		clipboardData.setData('text', text);
		const event = new ClipboardEvent('paste', { clipboardData, bubbles: true, cancelable: true });
		getInput().dispatchEvent(event);
		return event;
	}

	function typeAndPressEnter(text: string) {
		const input = getInput();
		input.value = text;
		input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', bubbles: true }));
	}

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbTagsInputElement);
	});

	describe('paste handling', () => {
		it('splits comma-separated pasted text into multiple tags', async () => {
			dispatchPaste('one,two,three');
			await element.updateComplete;
			expect(element.items).to.deep.equal(['one', 'two', 'three']);
		});

		it('splits newline-separated pasted text into multiple tags', async () => {
			dispatchPaste('one\ntwo\r\nthree');
			await element.updateComplete;
			expect(element.items).to.deep.equal(['one', 'two', 'three']);
		});

		it('trims whitespace and drops empty segments', async () => {
			dispatchPaste(' one , , two ,');
			await element.updateComplete;
			expect(element.items).to.deep.equal(['one', 'two']);
		});

		it('appends pasted tags to existing ones and skips duplicates', async () => {
			element.items = ['one'];
			dispatchPaste('one,two');
			await element.updateComplete;
			expect(element.items).to.deep.equal(['one', 'two']);
		});

		it('de-duplicates repeated values within a single paste', async () => {
			dispatchPaste('one,one,two');
			await element.updateComplete;
			expect(element.items).to.deep.equal(['one', 'two']);
		});

		it('does not update or fire a change event when every pasted value already exists', async () => {
			element.items = ['one', 'two'];
			let changeCount = 0;
			element.addEventListener(UmbChangeEvent.TYPE, () => changeCount++);
			dispatchPaste('one,two');
			await element.updateComplete;
			expect(element.items).to.deep.equal(['one', 'two']);
			expect(changeCount).to.equal(0);
		});

		it('clears the input after a paste that adds tags', async () => {
			const input = getInput();
			input.value = 'hello';
			input.dispatchEvent(new InputEvent('input', { bubbles: true }));
			dispatchPaste('one,two');
			await element.updateComplete;
			expect(element.items).to.deep.equal(['one', 'two']);
			expect(getInput().value).to.equal('');
		});

		it('dispatches a change event when tags are added', async () => {
			let changeCount = 0;
			element.addEventListener(UmbChangeEvent.TYPE, () => changeCount++);
			dispatchPaste('one,two');
			await element.updateComplete;
			expect(changeCount).to.equal(1);
		});

		it('does not split pasted text without a separator', async () => {
			const event = dispatchPaste('single');
			await element.updateComplete;
			expect(element.items).to.have.lengthOf(0);
			expect(event.defaultPrevented).to.be.false;
		});
	});

	describe('typing', () => {
		it('creates a tag when a value is typed and Enter is pressed', async () => {
			typeAndPressEnter('hello');
			await element.updateComplete;
			expect(element.items).to.deep.equal(['hello']);
		});

		it('keeps a typed comma as a literal part of a single tag', async () => {
			typeAndPressEnter('one, two');
			await element.updateComplete;
			expect(element.items).to.deep.equal(['one, two']);
		});

		it('trims the typed value before creating the tag', async () => {
			typeAndPressEnter('  hello  ');
			await element.updateComplete;
			expect(element.items).to.deep.equal(['hello']);
		});

		it('does not create a tag from an empty or whitespace-only value', async () => {
			typeAndPressEnter('   ');
			await element.updateComplete;
			expect(element.items).to.have.lengthOf(0);
		});

		it('does not add a duplicate of an existing tag', async () => {
			element.items = ['hello'];
			typeAndPressEnter('hello');
			await element.updateComplete;
			expect(element.items).to.deep.equal(['hello']);
		});

		it('dispatches a change event when a tag is created', async () => {
			let changeCount = 0;
			element.addEventListener(UmbChangeEvent.TYPE, () => changeCount++);
			typeAndPressEnter('hello');
			await element.updateComplete;
			expect(changeCount).to.equal(1);
		});
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
