import { expect } from '@open-wc/testing';
import { UmbVariantValidationToHintsManager } from './variant-validation-to-hints.manager.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbHintController, type UmbVariantHint } from '@umbraco-cms/backoffice/hint';
import { UmbDataPathVariantQuery, UmbValidationController } from '@umbraco-cms/backoffice/validation';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

@customElement('umb-test-variant-validation-to-hints-host')
export class UmbTestVariantValidationToHintsHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

const variantNamePath = (variantId: UmbVariantId) => `$.variants[${UmbDataPathVariantQuery(variantId)}].name`;

describe('UmbVariantValidationToHintsManager', () => {
	let host: UmbTestVariantValidationToHintsHostElement;
	let validation: UmbValidationController;
	let hints: UmbHintController<UmbVariantHint>;

	beforeEach(() => {
		host = new UmbTestVariantValidationToHintsHostElement();
		validation = new UmbValidationController(host);
		hints = new UmbHintController<UmbVariantHint>(host);
		new UmbVariantValidationToHintsManager(host, validation, hints);
	});

	afterEach(() => {
		host.destroy();
	});

	it('adds a hint for the variant of a variant validation message, outside of any workspace view', () => {
		const variantId = new UmbVariantId('da-dk', null);
		validation.messages.addMessage('custom', variantNamePath(variantId), 'error', 'msg-1');

		const all = hints.getAll();
		expect(all).to.have.length(1);
		expect(all[0].unique).to.equal('msg-1');
		expect(all[0].path).to.deep.equal([]);
		expect(all[0].variantId?.compare(variantId)).to.be.true;
	});

	it('removes the hint when the validation message is removed', () => {
		validation.messages.addMessage('custom', variantNamePath(new UmbVariantId('da-dk', null)), 'error', 'msg-1');

		validation.messages.removeMessageByKey('msg-1');

		expect(hints.getAll()).to.be.empty;
	});

	it('ignores validation messages outside the variants data', () => {
		validation.messages.addMessage('custom', `$.values[?(@.alias == 'title')].value`, 'error', 'msg-1');

		expect(hints.getAll()).to.be.empty;
	});
});
