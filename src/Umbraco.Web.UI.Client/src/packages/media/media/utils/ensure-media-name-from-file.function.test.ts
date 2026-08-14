import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import { ensureMediaNameFromFile } from './ensure-media-name-from-file.function.js';
import { expect } from '@open-wc/testing';
import type { UmbClassInterface } from '@umbraco-cms/backoffice/class-api';

interface DatasetStub {
	getEntityType: () => string;
	getName: () => string | undefined;
	setName: (value: string) => void;
}

function makeHost(datasetContext: DatasetStub | undefined): { host: UmbClassInterface; setNameCalls: string[] } {
	const setNameCalls: string[] = [];
	const wrappedDataset = datasetContext
		? {
				...datasetContext,
				setName: (value: string) => {
					setNameCalls.push(value);
					datasetContext.setName(value);
				},
			}
		: undefined;
	const host = {
		getContext: async () => wrappedDataset,
	} as unknown as UmbClassInterface;
	return { host, setNameCalls };
}

describe('ensureMediaNameFromFile', () => {
	it('sets the friendly name when no name is present on a media item', async () => {
		const { host, setNameCalls } = makeHost({
			getEntityType: () => UMB_MEDIA_ENTITY_TYPE,
			getName: () => '',
			setName: () => {},
		});

		await ensureMediaNameFromFile(host, new File([''], 'my-image-file.jpg'));

		expect(setNameCalls).to.deep.equal(['My Image File']);
	});

	it('sets the friendly name when the current name is whitespace only', async () => {
		const { host, setNameCalls } = makeHost({
			getEntityType: () => UMB_MEDIA_ENTITY_TYPE,
			getName: () => '   ',
			setName: () => {},
		});

		await ensureMediaNameFromFile(host, new File([''], 'photo.png'));

		expect(setNameCalls).to.deep.equal(['Photo']);
	});

	it('does not overwrite a name the user has already typed', async () => {
		const { host, setNameCalls } = makeHost({
			getEntityType: () => UMB_MEDIA_ENTITY_TYPE,
			getName: () => 'User typed name',
			setName: () => {},
		});

		await ensureMediaNameFromFile(host, new File([''], 'photo.png'));

		expect(setNameCalls).to.deep.equal([]);
	});

	it('does nothing when the dataset context is for a non-media entity', async () => {
		const { host, setNameCalls } = makeHost({
			getEntityType: () => 'document',
			getName: () => '',
			setName: () => {},
		});

		await ensureMediaNameFromFile(host, new File([''], 'photo.png'));

		expect(setNameCalls).to.deep.equal([]);
	});

	it('does nothing when no dataset context is available', async () => {
		const { host, setNameCalls } = makeHost(undefined);

		await ensureMediaNameFromFile(host, new File([''], 'photo.png'));

		expect(setNameCalls).to.deep.equal([]);
	});

	it('does not reject when getContext rejects (e.g. on timeout)', async () => {
		const host = {
			getContext: async () => {
				throw new Error('context not found');
			},
		} as unknown as UmbClassInterface;

		await ensureMediaNameFromFile(host, new File([''], 'photo.png'));
	});
});
