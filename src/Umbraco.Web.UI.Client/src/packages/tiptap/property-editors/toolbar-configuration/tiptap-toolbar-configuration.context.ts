import type { UmbTiptapToolbarValue } from '../../components/types.js';
import type {
	UmbTiptapToolbarExtension,
	UmbTiptapToolbarGroupViewModel,
	UmbTiptapToolbarRowViewModel,
} from '../types.js';
import { UMB_TIPTAP_TOOLBAR_CONFIGURATION_CONTEXT } from './tiptap-toolbar-configuration.context-token.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbTiptapToolbarConfigurationContext extends UmbContextBase {
	#extensions = new UmbArrayState<UmbTiptapToolbarExtension>([], (x) => x.alias);
	public readonly extensions = this.#extensions.asObservable();

	#reload = new UmbBooleanState(false);
	public readonly reload = this.#reload.asObservable();

	#extensionsEnabled = new Set<string>();

	#extensionsInUse = new Set<string>();

	#lookup?: Map<string, UmbTiptapToolbarExtension>;

	#toolbar = new UmbArrayState<UmbTiptapToolbarRowViewModel>([], (x) => x.unique);
	public readonly toolbar = this.#toolbar.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_TIPTAP_TOOLBAR_CONFIGURATION_CONTEXT);

		this.observe(umbExtensionsRegistry.byType('tiptapToolbarExtension'), (extensions) => {
			const _extensions = extensions
				.sort((a, b) => a.alias.localeCompare(b.alias))
				.map((ext) => ({
					kind: (ext.kind as string) ?? 'button',
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

	public cloneToolbarValue(value?: UmbTiptapToolbarValue | null): UmbTiptapToolbarValue {
		if (!this.isValidToolbarValue(value)) return [[[]]];
		return value.map((row) => row.map((group) => [...group]));
	}

	public filterExtensions(query: string): Array<UmbTiptapToolbarExtension> {
		return this.#extensions
			.getValue()
			.filter((ext) => ext.alias?.toLowerCase().includes(query) || ext.label?.toLowerCase().includes(query));
	}

	public getExtensionByAlias(alias: string): UmbTiptapToolbarExtension | undefined {
		return this.#lookup?.get(alias);
	}

	public isExtensionEnabled(alias: string): boolean {
		return this.#extensionsEnabled.has(alias);
	}

	public isExtensionInUse(alias: string): boolean {
		return this.#extensionsInUse.has(alias);
	}

	public isValidToolbarValue(value: unknown): value is UmbTiptapToolbarValue {
		if (!Array.isArray(value)) return false;
		for (const row of value) {
			if (!Array.isArray(row)) return false;
			for (const group of row) {
				if (!Array.isArray(group)) return false;
				for (const alias of group) {
					if (typeof alias !== 'string') return false;
				}
			}
		}
		return true;
	}

	public insertToolbarItem(alias: string, to: [number, number, number]) {
		const toolbar = [...this.#toolbar.getValue()];

		const [rowIndex, groupIndex, itemIndex] = to;

		const row = toolbar[rowIndex];
		const rowData = [...row.data];
		const group = rowData[groupIndex];
		const items = [...group.data];

		items.splice(itemIndex, 0, alias);
		this.#extensionsInUse.add(alias);

		rowData[groupIndex] = { unique: group.unique, data: items };
		toolbar[rowIndex] = { unique: row.unique, data: rowData };

		this.#toolbar.setValue(toolbar);
	}

	public insertToolbarGroup(rowIndex: number, groupIndex: number) {
		const toolbar = [...this.#toolbar.getValue()];
		const row = toolbar[rowIndex];
		const groups = [...row.data];
		groups.splice(groupIndex, 0, { unique: UmbId.new(), data: [] });
		toolbar[rowIndex] = { unique: row.unique, data: groups };
		this.#toolbar.setValue(toolbar);
	}

	public insertToolbarRow(rowIndex: number) {
		const toolbar = [...this.#toolbar.getValue()];
		toolbar.splice(rowIndex, 0, { unique: UmbId.new(), data: [{ unique: UmbId.new(), data: [] }] });
		this.#toolbar.setValue(toolbar);
	}

	public moveToolbarItem(from: [number, number, number], to: [number, number, number]) {
		const [fromRowIndex, fromGroupIndex, fromItemIndex] = from;
		const [toRowIndex, toGroupIndex, toItemIndex] = to;

		const toolbar = [...this.#toolbar.getValue()];

		const fromRow = toolbar[fromRowIndex];
		const fromRowData = [...fromRow.data];
		const fromGroup = fromRowData[fromGroupIndex];
		const fromItems = [...fromGroup.data];

		const toBeMoved = fromItems.splice(fromItemIndex, 1);

		fromRowData[fromGroupIndex] = { unique: fromGroup.unique, data: fromItems };
		toolbar[fromRowIndex] = { unique: fromRow.unique, data: fromRowData };

		const toRow = toolbar[toRowIndex];
		const toRowData = [...toRow.data];
		const toGroup = toRowData[toGroupIndex];
		const toItems = [...toGroup.data];

		toItems.splice(toItemIndex, 0, toBeMoved[0]);

		toRowData[toGroupIndex] = { unique: toGroup.unique, data: toItems };
		toolbar[toRowIndex] = { unique: toRow.unique, data: toRowData };

		this.#toolbar.setValue(toolbar);
	}

	public removeToolbarItem(from: [number, number, number]) {
		const [rowIndex, groupIndex, itemIndex] = from;

		const toolbar = [...this.#toolbar.getValue()];
		const row = toolbar[rowIndex];
		const rowData = [...row.data];
		const group = rowData[groupIndex];
		const items = [...group.data];

		const removed = items.splice(itemIndex, 1);
		removed.forEach((alias) => this.#extensionsInUse.delete(alias));

		rowData[groupIndex] = { unique: group.unique, data: items };
		toolbar[rowIndex] = { unique: row.unique, data: rowData };

		this.#toolbar.setValue(toolbar);
	}

	public removeToolbarGroup(rowIndex: number, groupIndex: number) {
		const toolbar = [...this.#toolbar.getValue()];

		if (toolbar[rowIndex].data.length > groupIndex) {
			const row = toolbar[rowIndex];
			const groups = [...row.data];
			const removed = groups.splice(groupIndex, 1);
			removed.forEach((group) => group.data.forEach((alias) => this.#extensionsInUse.delete(alias)));
			toolbar[rowIndex] = { unique: row.unique, data: groups };
		}

		// Prevent leaving an empty group
		if (toolbar[rowIndex].data.length === 0) {
			toolbar[rowIndex].data[0] = { unique: UmbId.new(), data: [] };
		}

		this.#toolbar.setValue(toolbar);
	}

	public removeToolbarRow(rowIndex: number) {
		const toolbar = [...this.#toolbar.getValue()];

		if (toolbar.length > rowIndex) {
			const removed = toolbar.splice(rowIndex, 1);
			removed.forEach((row) =>
				row.data.forEach((group) => group.data.forEach((alias) => this.#extensionsInUse.delete(alias))),
			);
		}

		// Prevent leaving an empty row
		if (toolbar.length === 0) {
			toolbar[0] = { unique: UmbId.new(), data: [{ unique: UmbId.new(), data: [] }] };
		}

		this.#toolbar.setValue(toolbar);
	}

	public setToolbar(value?: UmbTiptapToolbarValue | null) {
		if (!this.isValidToolbarValue(value)) {
			value = [[[]]];
		}

		this.#extensionsInUse.clear();
		value.forEach((row) => row.forEach((group) => group.forEach((alias) => this.#extensionsInUse.add(alias))));

		const toolbar = value.map((row) => ({
			unique: UmbId.new(),
			data: row.map((group) => ({ unique: UmbId.new(), data: group })),
		}));

		this.#toolbar.setValue(toolbar);
	}

	public updateToolbarItem(aliases: Array<string>, to: [number, number]) {
		const toolbar = [...this.#toolbar.getValue()];
		const [rowIndex, groupIndex] = to;

		const newToolbar = toolbar.map((row, rIdx) => {
			if (rIdx !== rowIndex) return row;
			return {
				...row,
				data: row.data.map((group, gIdx) => {
					if (gIdx !== groupIndex) return group;
					return {
						...group,
						data: [...aliases],
					};
				}),
			};
		});

		this.#toolbar.setValue(newToolbar);
	}

	public updateToolbarRow(rowIndex: number, groups: Array<UmbTiptapToolbarGroupViewModel>) {
		const toolbar = [...this.#toolbar.getValue()];

		const row = toolbar[rowIndex];
		toolbar[rowIndex] = { unique: row.unique, data: groups };

		this.#toolbar.setValue(toolbar);
	}
}
