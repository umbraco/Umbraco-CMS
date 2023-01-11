import { UmbWorkspaceContentContext } from '../../../shared/components/workspace/workspace-content/workspace-content.context';
import type { UmbDataTypeStore, UmbDataTypeStoreItemType } from 'src/backoffice/settings/data-types/data-type.store';
import type { DataTypeDetails } from '@umbraco-cms/models';
import { UmbControllerHostInterface } from 'src/core/controller/controller-host.mixin';
import { appendToFrozenArray } from 'src/core/observable-api/unique-behavior-subject';

const DefaultDataTypeData = {
	key: '',
	name: '',
	icon: '',
	type: 'data-type',
	hasChildren: false,
	parentKey: '',
	propertyEditorModelAlias: '',
	propertyEditorUIAlias: '',
	data: [],
} as UmbDataTypeStoreItemType;

export class UmbWorkspaceDataTypeContext extends UmbWorkspaceContentContext<
	UmbDataTypeStoreItemType,
	UmbDataTypeStore
> {
	constructor(host: UmbControllerHostInterface) {
		super(host, DefaultDataTypeData, 'umbDataTypeStore', 'dataType');
	}

	public setPropertyValue(alias: string, value: unknown) {

		// TODO: make sure to check that we have a details model? otherwise fail? 8This can be relevant if we use the same context for tree actions?
		const entry = {alias: alias, value: value};

		const newDataSet = appendToFrozenArray((this._data.getValue() as DataTypeDetails).data, entry, x => x.alias === alias);

		this.update({data: newDataSet});
	}
}
