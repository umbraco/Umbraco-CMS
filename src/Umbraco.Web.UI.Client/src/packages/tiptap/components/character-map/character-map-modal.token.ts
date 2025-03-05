import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbCharacterMapModalData = never;

export type UmbCharacterMapModalValue = string;

export const UMB_CHARACTER_MAP_MODAL = new UmbModalToken<UmbCharacterMapModalData, UmbCharacterMapModalValue>(
	'Umb.Modal.CharacterMap',
	{
		modal: { size: 'medium', type: 'dialog' },
	},
);
