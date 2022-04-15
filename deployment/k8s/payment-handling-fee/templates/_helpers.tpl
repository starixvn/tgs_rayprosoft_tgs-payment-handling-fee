{{/*
Expand the name of the chart.
*/}}
{{- define "payment-handling-fee.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "payment-handling-fee.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "payment-handling-fee.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "payment-handling-fee.labels" -}}
helm.sh/chart: {{ include "payment-handling-fee.chart" . }}
{{ include "payment-handling-fee.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "payment-handling-fee.selectorLabels" -}}
app.kubernetes.io/name: {{ include "payment-handling-fee.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app: {{ include "payment-handling-fee.name" . }}
tgs.kind: payment
{{- end }}

{{/*
Create the name of the service account to use
*/}}
{{- define "payment-handling-fee.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "payment-handling-fee.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}
