import { toMediaPickerInputMemories, toMediaPickerScopeMemory } from './media-picker-memory-relay.function.js';
import { expect } from '@open-wc/testing';

const MEDIA_PICKER_MEMORY_UNIQUE = 'UmbPickerModal:Umb.Modal.MediaPicker';

describe('toMediaPickerInputMemories', () => {
	it('relays the scope wrapper entry unchanged, as a single-item array', () => {
		const scopeMemory = {
			unique: MEDIA_PICKER_MEMORY_UNIQUE,
			memories: [{ unique: 'UmbMediaItemPickerLocation', value: { entity: { unique: 'folder-1' } } }],
		};

		const result = toMediaPickerInputMemories(scopeMemory);

		// Must NOT unwrap to the nested `.memories` array — `<umb-input-media>`'s own scope expects
		// to find the same wrapper shape (keyed by modal alias) that any other scope holds, since the
		// media-picker modal it opens reads from that scope the same way regardless of which scope it is.
		expect(result).to.deep.equal([scopeMemory]);
	});

	it('returns an empty array when there is nothing remembered yet', () => {
		expect(toMediaPickerInputMemories(undefined)).to.deep.equal([]);
	});
});

describe('toMediaPickerScopeMemory', () => {
	it('picks out the wrapper entry matching the given unique, unchanged', () => {
		const wrapper = {
			unique: MEDIA_PICKER_MEMORY_UNIQUE,
			memories: [{ unique: 'UmbMediaItemPickerLocation', value: { entity: { unique: 'folder-2' } } }],
		};
		const inputMemories = [wrapper];

		const result = toMediaPickerScopeMemory(inputMemories, MEDIA_PICKER_MEMORY_UNIQUE);

		// Must NOT re-wrap the whole array under the same key again — that would nest the wrapper
		// inside itself and corrupt the outer scope for every other reader of that key.
		expect(result).to.deep.equal(wrapper);
	});

	it('returns undefined when no entry matches the unique', () => {
		expect(toMediaPickerScopeMemory([{ unique: 'SomethingElse', memories: [] }], MEDIA_PICKER_MEMORY_UNIQUE)).to.be
			.undefined;
	});

	it('returns undefined for an empty or missing list', () => {
		expect(toMediaPickerScopeMemory([], MEDIA_PICKER_MEMORY_UNIQUE)).to.be.undefined;
		expect(toMediaPickerScopeMemory(undefined, MEDIA_PICKER_MEMORY_UNIQUE)).to.be.undefined;
	});
});
