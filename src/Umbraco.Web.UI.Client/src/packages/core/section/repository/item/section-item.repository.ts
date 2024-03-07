import type { UmbSectionItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { ManifestSection } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { map } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbSectionItemRepository extends UmbRepositoryBase implements UmbItemRepository<UmbSectionItemModel> {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	/**
	 * Requests the items for the given uniques
	 * @param {Array<string>} uniques
	 * @return {*}
	 * @memberof UmbItemRepositoryBase
	 */
	async requestItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const sectionManifests = umbExtensionsRegistry.getAllExtensions().filter((x) => x.type === 'section');
		const sectionItems: Array<UmbSectionItemModel> = sectionManifests.map((manifest) => itemMapper(manifest));

		const sectionItemsObservable = umbExtensionsRegistry
			.byType('section')
			.pipe(map((manifests) => manifests.map((manifest) => itemMapper(manifest))));

		return { data: sectionItems, asObservable: () => sectionItemsObservable };
	}

	/**
	 * Returns a promise with an observable of the items for the given uniques
	 * @param {Array<string>} uniques
	 * @return {*}
	 * @memberof UmbItemRepositoryBase
	 */
	async items(uniques: Array<string>) {
		return umbExtensionsRegistry
			.getAllExtensions()
			.filter((x) => x.type === 'section')
			.map((manifest) => itemMapper(manifest))
			.filter((x) => uniques.includes(x.unique));
	}
}

const itemMapper = (manifest: ManifestSection): UmbSectionItemModel => {
	return {
		...manifest,
		unique: manifest.alias,
	};
};

export default UmbSectionItemRepository;
