import { manifests } from './manifests.js';
import { expect } from '@open-wc/testing';

describe('Document Picker property editor UI manifests', () => {
	describe('clear', () => {
		it('should register a clear property action for the document picker', () => {
			const clearAction = manifests.find(
				(manifest) =>
					manifest.type === 'propertyAction' &&
					'kind' in manifest &&
					manifest.kind === 'clear' &&
					'forPropertyEditorUis' in manifest &&
					manifest.forPropertyEditorUis.includes('Umb.PropertyEditorUi.DocumentPicker'),
			);
			expect(clearAction).to.not.be.undefined;
		});
	});
});
