import {
	UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_ALIAS,
	UMB_MODAL_TEMPLATING_INSERT_FIELD_SIDEBAR_ALIAS,
} from './manifests';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_MODAL = new UmbModalToken<{ hidePartialView: boolean }>(
	UMB_MODAL_TEMPLATING_INSERT_CHOOSE_TYPE_SIDEBAR_ALIAS,
	{
		type: 'sidebar',
		size: 'small',
	}
);

export const UMB_MODAL_TEMPLATING_INSERT_FIELD_SIDEBAR_MODAL = new UmbModalToken(
	UMB_MODAL_TEMPLATING_INSERT_FIELD_SIDEBAR_ALIAS,
	{
		type: 'sidebar',
		size: 'small',
	}
);
