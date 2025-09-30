import type { DomainPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCultureAndHostnamesModalData {
	unique: string | null;
}

export interface UmbCultureAndHostnamesModalValue {
	defaultIsoCode?: string | null;
	domains: Array<DomainPresentationModel>;
}

export const UMB_CULTURE_AND_HOSTNAMES_MODAL = new UmbModalToken<
	UmbCultureAndHostnamesModalData,
	UmbCultureAndHostnamesModalValue
>('Umb.Modal.CultureAndHostnames', {
	modal: {
		type: 'sidebar',
		size: 'medium',
	},
});
