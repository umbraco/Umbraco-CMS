import { UmbPickerContext } from '@umbraco-cms/backoffice/picker';
import { UmbDataTypeRepository } from '../../repository/data-type.repository';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UMB_DATA_TYPE_PICKER_MODAL } from '@umbraco-cms/backoffice/modal';

export class UmbDataTypePickerContext extends UmbPickerContext<UmbDataTypeRepository> {
	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Repository.DataType', UMB_DATA_TYPE_PICKER_MODAL);
	}
}
