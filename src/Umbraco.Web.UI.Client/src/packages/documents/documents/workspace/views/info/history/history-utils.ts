import { AuditTypeModel } from '@umbraco-cms/backoffice/backend-api';

interface HistoricStyleMap {
	look: 'default' | 'primary' | 'secondary' | 'outline' | 'placeholder';
	color: 'default' | 'danger' | 'warning' | 'positive';
}

interface HistoricText {
	label: string;
	desc: string;
}

interface HistoricData {
	style: HistoricStyleMap;
	text: HistoricText;
}

// Return label, color, look, desc

export function HistoricTagAndDescription(type: AuditTypeModel): HistoricData {
	switch (type) {
		case AuditTypeModel.SAVE:
			return {
				style: { look: 'primary', color: 'default' },
				text: { label: 'auditTrails_smallSave', desc: 'auditTrails_save' },
			};

		case AuditTypeModel.PUBLISH:
			return {
				style: { look: 'primary', color: 'positive' },
				text: { label: 'content_publish', desc: 'auditTrails_publish' },
			};

		case AuditTypeModel.UNPUBLISH:
			return {
				style: { look: 'primary', color: 'warning' },
				text: { label: 'content_unpublish', desc: 'auditTrails_unpublish' },
			};

		case AuditTypeModel.CONTENT_VERSION_ENABLE_CLEANUP:
			return {
				style: { look: 'secondary', color: 'default' },
				text: {
					label: 'contentTypeEditor_historyCleanupEnableCleanup',
					desc: 'auditTrails_contentversionenablecleanup',
				},
			};

		case AuditTypeModel.CONTENT_VERSION_PREVENT_CLEANUP:
			return {
				style: { look: 'secondary', color: 'default' },
				text: {
					label: 'contentTypeEditor_historyCleanupPreventCleanup',
					desc: 'auditTrails_contentversionpreventcleanup',
				},
			};

		default:
			return {
				style: { look: 'placeholder', color: 'danger' },
				text: { label: type, desc: 'TODO' },
			};
	}
}
