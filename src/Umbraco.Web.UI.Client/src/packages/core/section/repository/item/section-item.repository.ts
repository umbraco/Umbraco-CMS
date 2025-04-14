import type { UmbSectionItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { createObservablePart } from '@umbraco-cms/backoffice/observable-api';
import type { ManifestSection } from '../../extensions/index.js';

export class UmbSectionItemRepository extends UmbRepositoryBase implements UmbItemRepository<UmbSectionItemModel> {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	/**
	 * Requests the items for the given uniques
	 * @param {Array<string>} uniques
	 * @returns {*}
	 * @memberof UmbItemRepositoryBase
	 */
	async requestItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const sectionManifests = umbExtensionsRegistry
			.getAllExtensions()
			.filter((manifest) => manifest.type === 'section')
			.filter((manifest) => uniques.includes(manifest.alias)) as Array<ManifestSection>;

		const sectionItems: Array<UmbSectionItemModel> = sectionManifests.map((manifest) => itemMapper(manifest));

		return { data: sectionItems, asObservable: () => sectionItemsByUniquesObservable(uniques) };
	}

	/**
	 * Returns a promise with an observable of the items for the given uniques
	 * @param {Array<string>} uniques
	 * @returns {*}
	 * @memberof UmbItemRepositoryBase
	 */
	async items(uniques: Array<string>) {
		return sectionItemsByUniquesObservable(uniques);
	}
}

const sectionItemsObservable = createObservablePart(umbExtensionsRegistry.byType('section'), (manifests) =>
	manifests.map((manifest) => itemMapper(manifest)),
);

const sectionItemsByUniquesObservable = (uniques: Array<string>) =>
	createObservablePart(sectionItemsObservable, (items) => items.filter((x) => uniques.includes(x.unique)));

const itemMapper = (manifest: ManifestSection): UmbSectionItemModel => {
	return {
		...manifest,
		unique: manifest.alias,
	};
};

export default UmbSectionItemRepository;
