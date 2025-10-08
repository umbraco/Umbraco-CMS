import type { UmbTiptapStatusbarExtension, UmbTiptapStatusbarViewModel } from '../types.js';
import type { UmbTiptapStatusbarValue } from '../../components/types.js';
import { UMB_TIPTAP_STATUSBAR_CONFIGURATION_CONTEXT } from './tiptap-statusbar-configuration.context-token.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbTiptapStatusbarConfigurationContext extends UmbContextBase {
	#extensions = new UmbArrayState<UmbTiptapStatusbarExtension>([], (x) => x.alias);
	public readonly extensions = this.#extensions.asObservable();

	#reload = new UmbBooleanState(false);
	public readonly reload = this.#reload.asObservable();

	#extensionsEnabled = new Set<string>();

	#extensionsInUse = new Set<string>();

	#lookup?: Map<string, UmbTiptapStatusbarExtension>;

	#statusbar = new UmbArrayState<UmbTiptapStatusbarViewModel>([], (x) => x.unique);
	public readonly statusbar = this.#statusbar.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_TIPTAP_STATUSBAR_CONFIGURATION_CONTEXT);

		this.observe(umbExtensionsRegistry.byType('tiptapStatusbarExtension'), (extensions) => {
			const _extensions = extensions
				.sort((a, b) => a.alias.localeCompare(b.alias))
				.map((ext) => ({
					alias: ext.alias,
					label: ext.meta.label,
					icon: ext.meta.icon,
					dependencies: ext.forExtensions,
				}));

			this.#extensions.setValue(_extensions);

			this.#lookup = new Map(_extensions.map((ext) => [ext.alias, ext]));
		});

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (dataset) => {
			this.observe(
				await dataset?.propertyValueByAlias<Array<string>>('extensions'),
				(extensions) => {
					if (extensions) {
						this.#extensionsEnabled.clear();
						this.#reload.setValue(false);

						this.#extensions
							.getValue()
							.filter((x) => !x.dependencies || x.dependencies.every((z) => extensions.includes(z)))
							.map((x) => x.alias)
							.forEach((alias) => this.#extensionsEnabled.add(alias));

						this.#reload.setValue(true);
					}
				},
				'_observeExtensions',
			);
		});
	}

	public cloneStatusbarValue(value?: UmbTiptapStatusbarValue | null): UmbTiptapStatusbarValue {
		if (!this.isValidStatusbarValue(value)) return [[], []];
		return value.map((area) => [...area]);
	}

	public filterExtensions(query: string): Array<UmbTiptapStatusbarExtension> {
		return this.#extensions
			.getValue()
			.filter((ext) => ext.alias?.toLowerCase().includes(query) || ext.label?.toLowerCase().includes(query));
	}

	public getExtensionByAlias(alias: string): UmbTiptapStatusbarExtension | undefined {
		return this.#lookup?.get(alias);
	}

	public isExtensionEnabled(alias: string): boolean {
		return this.#extensionsEnabled.has(alias);
	}

	public isExtensionInUse(alias: string): boolean {
		return this.#extensionsInUse.has(alias);
	}

	public isValidStatusbarValue(value: unknown): value is UmbTiptapStatusbarValue {
		if (!Array.isArray(value)) return false;
		for (const area of value) {
			if (!Array.isArray(area)) return false;
			for (const alias of area) {
				if (typeof alias !== 'string') return false;
			}
		}
		return true;
	}

	public insertStatusbarItem(alias: string, to: [number, number]) {
		const statusbar = [...this.#statusbar.getValue()];

		const [areaIndex, itemIndex] = to;

		const area = statusbar[areaIndex];
		const items = [...area.data];

		items.splice(itemIndex, 0, alias);
		this.#extensionsInUse.add(alias);

		statusbar[areaIndex] = { unique: area.unique, data: items };

		this.#statusbar.setValue(statusbar);
	}

	public moveStatusbarItem(from: [number, number], to: [number, number]) {
		const [fromAreaIndex, fromItemIndex] = from;
		const [toAreaIndex, toItemIndex] = to;

		const statusbar = [...this.#statusbar.getValue()];

		const fromArea = statusbar[fromAreaIndex];
		const fromItems = [...fromArea.data];

		const toBeMoved = fromItems.splice(fromItemIndex, 1);

		statusbar[fromAreaIndex] = { unique: fromArea.unique, data: fromItems };

		const toArea = statusbar[toAreaIndex];
		const toItems = [...toArea.data];

		toItems.splice(toItemIndex, 0, toBeMoved[0]);

		statusbar[toAreaIndex] = { unique: toArea.unique, data: toItems };

		this.#statusbar.setValue(statusbar);
	}

	public removeStatusbarItem(from: [number, number]) {
		const [areaIndex, itemIndex] = from;

		const statusbar = [...this.#statusbar.getValue()];

		const area = statusbar[areaIndex];
		const items = [...area.data];

		const removed = items.splice(itemIndex, 1);
		removed.forEach((alias) => this.#extensionsInUse.delete(alias));

		statusbar[areaIndex] = { unique: area.unique, data: items };

		this.#statusbar.setValue(statusbar);
	}

	public setStatusbar(value?: UmbTiptapStatusbarValue | null) {
		if (!this.isValidStatusbarValue(value)) {
			value = [[], []];
		}

		if (value.length === 1) {
			value = [[], value[0]];
		}

		this.#extensionsInUse.clear();
		value.forEach((area) => area.forEach((alias) => this.#extensionsInUse.add(alias)));

		const statusbar = value.map((area) => ({ unique: UmbId.new(), data: area }));

		this.#statusbar.setValue(statusbar);
	}
}
