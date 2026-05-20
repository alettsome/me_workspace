const state = {
  activeConversation: null,
  activeConversationId: null,
  activeDocument: null,
  activeJournalDetail: null,
  activeJournalDetailRequestId: 0,
  activeJournalEntryId: null,
  attachedFilePaths: [],
  conversations: [],
  editingMemoryId: null,
  filePreview: null,
  fileTree: [],
  chatListCollapsed: false,
  dictationActive: false,
  dictationAudioContext: null,
  dictationAudioProcessor: null,
  dictationAudioSource: null,
  dictationFinalText: "",
  dictationForceNextSync: false,
  dictationLastSyncedSampleCount: 0,
  dictationMediaRecorder: null,
  dictationPcmChunks: [],
  dictationRecognition: null,
  dictationSampleRate: 0,
  dictationSessionId: null,
  dictationSeedText: "",
  dictationSyncInFlight: false,
  dictationSyncRequested: false,
  dictationSyncTimer: null,
  dictationStopping: false,
  dictationStream: null,
  journalDetailCache: {},
  journalEntries: [],
  journalSelection: null,
  memoryItems: [],
  suppressRecentJournalFallback: false,
  systemFlow: null,
};

const elements = {
  appSummary: document.getElementById("app-summary"),
  attachedFiles: document.getElementById("attached-files"),
  chatForm: document.getElementById("chat-form"),
  chatJournalContext: document.getElementById("chat-journal-context"),
  chatSwitcherPanel: document.getElementById("chat-switcher-panel"),
  chatSwitcherSummary: document.getElementById("chat-switcher-summary"),
  chatSwitcherToggle: document.getElementById("chat-switcher-toggle"),
  chatTitle: document.getElementById("chat-title"),
  composerContextStatus: document.getElementById("composer-context-status"),
  composerDropHint: document.getElementById("composer-drop-hint"),
  connectionList: document.getElementById("connection-list"),
  contextSummary: document.getElementById("context-summary"),
  conversationList: document.getElementById("conversation-list"),
  documentKind: document.getElementById("document-kind"),
  documentOutline: document.getElementById("document-outline"),
  documentOutlineMeta: document.getElementById("document-outline-meta"),
  documentSubtitle: document.getElementById("document-subtitle"),
  documentTitle: document.getElementById("document-title"),
  documentView: document.getElementById("document-view"),
  fileTree: document.getElementById("file-tree"),
  journalForm: document.getElementById("journal-form"),
  journalList: document.getElementById("journal-list"),
  journalSummaryInput: document.getElementById("journal-summary-input"),
  journalTitleInput: document.getElementById("journal-title-input"),
  memoryCancelButton: document.getElementById("memory-cancel-button"),
  memoryContentInput: document.getElementById("memory-content-input"),
  memoryForm: document.getElementById("memory-form"),
  memoryKeyInput: document.getElementById("memory-key-input"),
  memoryList: document.getElementById("memory-list"),
  memoryPinnedInput: document.getElementById("memory-pinned-input"),
  memorySaveButton: document.getElementById("memory-save-button"),
  messageInput: document.getElementById("message-input"),
  messageList: document.getElementById("message-list"),
  newChatButton: document.getElementById("new-chat-button"),
  voiceDraftButton: document.getElementById("voice-draft-button"),
};

const dragDataType = "application/x-me-workspace-file-path";

async function request(url, options = {}) {
  const response = await fetch(url, {
    headers: {
      "Content-Type": "application/json",
    },
    ...options,
  });

  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

function escapeHtml(value) {
  return String(value ?? "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#39;");
}

function mergeDictationText(baseText, ...segments) {
  const extraText = segments.filter(Boolean).join(" ").trim();
  if (!baseText) {
    return extraText;
  }

  if (!extraText) {
    return baseText;
  }

  return /\s$/.test(baseText)
    ? `${baseText}${extraText}`
    : `${baseText} ${extraText}`;
}

function updateVoiceDraftButton() {
  elements.voiceDraftButton.textContent = state.dictationActive ? "Recording" : "Dictation";
  elements.voiceDraftButton.setAttribute("title", state.dictationActive ? "Stop dictation" : "Start dictation");
  elements.voiceDraftButton.setAttribute("aria-label", state.dictationActive ? "Stop dictation" : "Start dictation");
  elements.voiceDraftButton.classList.toggle("recording", state.dictationActive);
}

function describeDictationError(error) {
  if (!error) {
    return "unknown error";
  }

  if (typeof error === "string") {
    return error;
  }

  if (error.name === "NotAllowedError" || error.name === "SecurityError") {
    return "microphone access was blocked";
  }

  if (error.name === "NotFoundError") {
    return "no microphone was found";
  }

  if (error.name === "NotReadableError") {
    return "the microphone is busy in another app";
  }

  if (error.message) {
    return error.message;
  }

  return "unknown error";
}

function applyDictationText(currentText, isFinal = false) {
  if (!currentText) {
    return;
  }

  if (isFinal) {
    state.dictationFinalText = currentText;
  }

  elements.messageInput.value = mergeDictationText(state.dictationSeedText, currentText);
}

function resetDictationState() {
  state.dictationActive = false;
  state.dictationForceNextSync = false;
  state.dictationLastSyncedSampleCount = 0;
  if (state.dictationAudioProcessor) {
    state.dictationAudioProcessor.onaudioprocess = null;
  }
  if (state.dictationAudioSource) {
    state.dictationAudioSource.disconnect();
  }
  if (state.dictationAudioProcessor) {
    state.dictationAudioProcessor.disconnect();
  }
  if (state.dictationAudioContext && state.dictationAudioContext.state !== "closed") {
    state.dictationAudioContext.close().catch(() => {});
  }
  state.dictationAudioContext = null;
  state.dictationAudioProcessor = null;
  state.dictationAudioSource = null;
  state.dictationFinalText = "";
  state.dictationMediaRecorder = null;
  state.dictationPcmChunks = [];
  state.dictationRecognition = null;
  state.dictationSampleRate = 0;
  state.dictationSessionId = null;
  state.dictationSeedText = "";
  state.dictationSyncInFlight = false;
  state.dictationSyncRequested = false;
  if (state.dictationSyncTimer) {
    window.clearInterval(state.dictationSyncTimer);
  }
  state.dictationSyncTimer = null;
  state.dictationStopping = false;
  if (state.dictationStream) {
    for (const track of state.dictationStream.getTracks()) {
      track.stop();
    }
  }
  state.dictationStream = null;
  updateVoiceDraftButton();
}

async function startVoiceSession() {
  return request("/api/voice/sessions/start", {
    method: "POST",
  });
}

async function stopVoiceSession(sessionId) {
  return request(`/api/voice/sessions/${sessionId}/stop`, {
    method: "POST",
  });
}

async function appendVoiceChunk(sessionId, audioBlob) {
  const response = await fetch(`/api/voice/sessions/${sessionId}/chunks`, {
    method: "POST",
    body: audioBlob,
  });

  if (!response.ok) {
    throw new Error(`Voice chunk upload failed: ${response.status}`);
  }

  return response.json();
}

function writeAscii(view, offset, value) {
  for (let index = 0; index < value.length; index += 1) {
    view.setUint8(offset + index, value.charCodeAt(index));
  }
}

function buildWavBlob(chunks, sampleRate) {
  const totalSampleCount = chunks.reduce((sum, chunk) => sum + chunk.length, 0);
  const wavBuffer = new ArrayBuffer(44 + (totalSampleCount * 2));
  const view = new DataView(wavBuffer);

  writeAscii(view, 0, "RIFF");
  view.setUint32(4, 36 + (totalSampleCount * 2), true);
  writeAscii(view, 8, "WAVE");
  writeAscii(view, 12, "fmt ");
  view.setUint32(16, 16, true);
  view.setUint16(20, 1, true);
  view.setUint16(22, 1, true);
  view.setUint32(24, sampleRate, true);
  view.setUint32(28, sampleRate * 2, true);
  view.setUint16(32, 2, true);
  view.setUint16(34, 16, true);
  writeAscii(view, 36, "data");
  view.setUint32(40, totalSampleCount * 2, true);

  let offset = 44;

  for (const chunk of chunks) {
    for (let index = 0; index < chunk.length; index += 1) {
      const sample = Math.max(-1, Math.min(1, chunk[index]));
      view.setInt16(offset, sample < 0 ? sample * 0x8000 : sample * 0x7fff, true);
      offset += 2;
    }
  }

  return new Blob([wavBuffer], { type: "audio/wav" });
}

function getPcmSampleCount(chunks) {
  return chunks.reduce((sum, chunk) => sum + chunk.length, 0);
}

async function flushLocalDictationSync() {
  if (state.dictationSyncInFlight || !state.dictationSessionId) {
    return;
  }

  state.dictationSyncInFlight = true;

  try {
    while (state.dictationSyncRequested || state.dictationForceNextSync) {
      const forceSync = state.dictationForceNextSync;
      state.dictationSyncRequested = false;
      state.dictationForceNextSync = false;

      const currentSampleCount = getPcmSampleCount(state.dictationPcmChunks);
      if (currentSampleCount === 0) {
        continue;
      }

      if (!forceSync && currentSampleCount <= state.dictationLastSyncedSampleCount) {
        continue;
      }

      const audioBlob = buildWavBlob([...state.dictationPcmChunks], state.dictationSampleRate || 16000);
      if (audioBlob.size <= 44) {
        continue;
      }

      const sessionState = await appendVoiceChunk(state.dictationSessionId, audioBlob);
      state.dictationLastSyncedSampleCount = currentSampleCount;
      applyDictationText(sessionState.transcript, sessionState.final);
    }
  } catch (error) {
    console.error(error);

    if (!state.dictationStopping) {
      alert("Local dictation could not refresh the live transcript.");
    }
  } finally {
    state.dictationSyncInFlight = false;

    if ((state.dictationSyncRequested || state.dictationForceNextSync) && state.dictationSessionId) {
      void flushLocalDictationSync();
    }
  }
}

function requestLocalDictationSync(forceSync = false) {
  if (!state.dictationSessionId) {
    return;
  }

  state.dictationSyncRequested = true;
  state.dictationForceNextSync = state.dictationForceNextSync || forceSync;
  void flushLocalDictationSync();
}

async function startLocalSessionDictation() {
  const AudioContextCtor = window.AudioContext || window.webkitAudioContext;

  if (!navigator.mediaDevices?.getUserMedia || !AudioContextCtor) {
    return false;
  }

  const session = await startVoiceSession();
  let stream;
  let audioContext = null;
  let source = null;
  let processor = null;

  try {
    stream = await navigator.mediaDevices.getUserMedia({ audio: true });

    audioContext = new AudioContextCtor();
    if (audioContext.state === "suspended") {
      await audioContext.resume();
    }
    source = audioContext.createMediaStreamSource(stream);
    processor = audioContext.createScriptProcessor(4096, 1, 1);
  } catch (error) {
    if (processor) {
      processor.disconnect();
    }
    if (source) {
      source.disconnect();
    }
    if (audioContext && audioContext.state !== "closed") {
      await audioContext.close().catch(() => {});
    }
    if (stream) {
      for (const track of stream.getTracks()) {
        track.stop();
      }
    }
    try {
      await stopVoiceSession(session.sessionId);
    } catch {
    }

    throw error;
  }
  const pcmChunks = [];

  processor.onaudioprocess = (event) => {
    if (!state.dictationActive || state.dictationStopping) {
      return;
    }

    const channelData = event.inputBuffer.getChannelData(0);
    pcmChunks.push(new Float32Array(channelData));
  };

  source.connect(processor);
  processor.connect(audioContext.destination);

  state.dictationSessionId = session.sessionId;
  state.dictationSeedText = elements.messageInput.value.trim();
  state.dictationAudioContext = audioContext;
  state.dictationAudioProcessor = processor;
  state.dictationAudioSource = source;
  state.dictationFinalText = "";
  state.dictationStream = stream;
  state.dictationPcmChunks = pcmChunks;
  state.dictationSampleRate = audioContext.sampleRate;
  state.dictationLastSyncedSampleCount = 0;
  state.dictationSyncInFlight = false;
  state.dictationSyncRequested = false;
  state.dictationForceNextSync = false;
  state.dictationSyncTimer = window.setInterval(() => {
    if (state.dictationActive && !state.dictationStopping) {
      requestLocalDictationSync();
    }
  }, 2500);
  state.dictationStopping = false;
  state.dictationActive = true;
  updateVoiceDraftButton();
  return true;
}

async function stopLocalSessionDictation() {
  const sessionId = state.dictationSessionId;

  try {
    if (state.dictationSyncTimer) {
      window.clearInterval(state.dictationSyncTimer);
      state.dictationSyncTimer = null;
    }

    if (state.dictationAudioProcessor) {
      state.dictationAudioProcessor.onaudioprocess = null;
      state.dictationAudioProcessor.disconnect();
    }

    if (state.dictationAudioSource) {
      state.dictationAudioSource.disconnect();
    }

    if (state.dictationAudioContext && state.dictationAudioContext.state !== "closed") {
      await state.dictationAudioContext.close();
    }

    if (sessionId) {
      requestLocalDictationSync(true);
      await flushLocalDictationSync();
    }

    if (sessionId) {
      const sessionState = await stopVoiceSession(sessionId);
      applyDictationText(sessionState.transcript, true);
    }
  } finally {
    resetDictationState();
    elements.messageInput.focus();
  }
}

async function stopActiveDictation() {
  state.dictationStopping = true;

  if (state.dictationMediaRecorder && state.dictationMediaRecorder.state !== "inactive") {
    state.dictationMediaRecorder.stop();
    return;
  }

  if (state.dictationAudioContext) {
    await stopLocalSessionDictation();
    return;
  }

  if (state.dictationRecognition) {
    state.dictationRecognition.stop();
    return;
  }

  resetDictationState();
}

function createBrowserSpeechRecognition() {
  const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
  if (!SpeechRecognition) {
    return null;
  }

  const recognition = new SpeechRecognition();
  recognition.continuous = false;
  recognition.interimResults = true;
  recognition.maxAlternatives = 1;
  recognition.lang = navigator.language || "en-GB";
  return recognition;
}

function startBrowserDictation() {
  const recognition = createBrowserSpeechRecognition();
  if (!recognition) {
    return false;
  }

  state.dictationRecognition = recognition;
  state.dictationSeedText = elements.messageInput.value.trim();
  state.dictationFinalText = "";
  state.dictationStopping = false;

  recognition.addEventListener("start", () => {
    state.dictationActive = true;
    updateVoiceDraftButton();
  });

  recognition.addEventListener("result", (event) => {
    let finalText = state.dictationFinalText;
    let interimText = "";

    for (let index = event.resultIndex; index < event.results.length; index += 1) {
      const candidate = event.results[index]?.[0]?.transcript?.trim();
      if (!candidate) {
        continue;
      }

      if (event.results[index].isFinal) {
        finalText = mergeDictationText(finalText, candidate);
      } else {
        interimText = mergeDictationText(interimText, candidate);
      }
    }

    state.dictationFinalText = finalText;
    elements.messageInput.value = mergeDictationText(state.dictationSeedText, finalText, interimText);
  });

  recognition.addEventListener("error", async (event) => {
    if (event.error === "aborted" && state.dictationStopping) {
      return;
    }

    if (event.error === "not-allowed" || event.error === "service-not-allowed") {
      alert("Microphone access is blocked. Allow microphone access in the browser, then try dictation again.");
      return;
    }

    if (event.error === "no-speech") {
      return;
    }

    try {
      await loadVoiceDraft();
    } catch (error) {
      showError(error);
    }
  });

  recognition.addEventListener("end", () => {
    resetDictationState();
    elements.messageInput.focus();
  });

  recognition.start();
  return true;
}

function formatInlineText(value) {
  return escapeHtml(value).replace(/`([^`]+)`/g, "<code>$1</code>");
}

function slugify(value) {
  return String(value ?? "")
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "") || "section";
}

function getJournalEntryById(entryId) {
  return state.journalEntries.find((entry) => entry.id === entryId) ?? null;
}

function inferJournalEntryFromFiles() {
  for (const relativePath of state.attachedFilePaths) {
    const match = /^Journals\/([^/]+)\//i.exec(relativePath);
    if (!match) {
      continue;
    }

    const slug = match[1];
    const entry = state.journalEntries.find((journalEntry) => journalEntry.slug === slug);
    if (entry) {
      return {
        entryId: entry.id,
        matchedPath: relativePath,
      };
    }
  }

  return null;
}

function resolveJournalSelection() {
  if (state.activeConversation?.journalEntryId) {
    return {
      entryId: state.activeConversation.journalEntryId,
      origin: "linked-chat",
      reason: "This chat is already linked to the journal and keeps using it.",
    };
  }

  if (state.activeJournalEntryId) {
    return {
      entryId: state.activeJournalEntryId,
      origin: "manual-selection",
      reason: "You selected this journal in the explorer.",
    };
  }

  const inferred = inferJournalEntryFromFiles();
  if (inferred) {
    return {
      entryId: inferred.entryId,
      origin: "attached-journal-file",
      reason: `A file from ${inferred.matchedPath} points at this journal.`,
    };
  }

  if (!state.activeConversationId && !state.suppressRecentJournalFallback && state.journalEntries.length > 0) {
    return {
      entryId: state.journalEntries[0].id,
      origin: "recent-journal",
      reason: "Using the latest journal by default.",
    };
  }

  return null;
}

function formatTimestamp(value) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return date.toLocaleString();
}

function buildJournalContextSummary(conversation) {
  const selection = state.journalSelection;
  const detail = state.activeJournalDetail;
  const title = conversation?.journalTitle
    ?? detail?.title
    ?? (selection ? getJournalEntryById(selection.entryId)?.title : null);

  if (conversation?.journalTitle) {
    return {
      header: `Journal: ${conversation.journalTitle}`,
      composer: "Linked to this chat.",
    };
  }

  if (selection && title) {
    return {
      header: `Journal: ${title}`,
      composer: selection.reason,
    };
  }

  return {
    header: "No journal linked",
    composer: "",
  };
}

function setActiveDocument(documentSelection) {
  state.activeDocument = documentSelection;
  renderDocument();
}

function setActiveJournalDocument(entryId) {
  if (!entryId) {
    return;
  }

  setActiveDocument({
    type: "journal",
    entryId,
  });
}

function setActiveFileDocument(relativePath) {
  setActiveDocument({
    type: "file",
    relativePath,
  });
}

function buildDocumentModel() {
  if (state.activeDocument?.type === "file") {
    if (!state.filePreview || state.filePreview.relativePath !== state.activeDocument.relativePath) {
      return {
        kind: "File",
        title: state.activeDocument.relativePath,
        subtitle: "Loading file preview...",
        content: "",
        tags: [],
        logs: [],
        summaryCards: [],
      };
    }

    const isAttached = state.attachedFilePaths.includes(state.filePreview.relativePath);
    return {
      kind: "File",
      title: state.filePreview.name,
      subtitle: state.filePreview.relativePath,
      content: state.filePreview.contentPreview,
      actionLabel: isAttached ? "Remove From Chat" : "Attach To Chat",
      actionPath: state.filePreview.relativePath,
      tags: [],
      logs: [],
      summaryCards: [
        {
          title: "Path",
          value: state.filePreview.relativePath,
        },
        {
          title: "Chat Context",
          value: isAttached ? "Attached to the next message." : "Not attached yet.",
        },
      ],
    };
  }

  const journalEntryId = state.activeDocument?.type === "journal"
    ? state.activeDocument.entryId
    : state.activeJournalDetail?.id;

  if (!journalEntryId) {
    return null;
  }

  const detail = state.activeJournalDetail?.id === journalEntryId
    ? state.activeJournalDetail
    : state.journalDetailCache[journalEntryId];

  if (!detail) {
    return {
      kind: "Journal Entry",
      title: getJournalEntryById(journalEntryId)?.title ?? "Journal",
      subtitle: "Loading journal detail...",
      content: "",
      actionLabel: null,
      actionPath: null,
      tags: [],
      logs: [],
      summaryCards: [],
    };
  }

  return {
    kind: "Journal Entry",
    title: detail.title,
    subtitle: `Updated ${formatTimestamp(detail.updatedUtc)}`,
    content: detail.entryContent,
    actionLabel: null,
    actionPath: null,
    tags: detail.tags,
    logs: detail.logs,
    summaryCards: [
      {
        title: "Summary",
        value: detail.summary || "No summary yet.",
      },
      {
        title: "Context",
        value: state.journalSelection?.entryId === detail.id
          ? state.journalSelection.reason
          : "Opened directly from the explorer.",
      },
    ],
  };
}

function parseDocumentContent(content) {
  const normalized = String(content ?? "").replaceAll("\r\n", "\n");
  const lines = normalized.split("\n");
  const blocks = [];
  const outline = [];
  let paragraphLines = [];
  let listItems = [];
  let codeLines = [];
  let inCodeBlock = false;
  let generatedCount = 0;

  function flushParagraph() {
    if (paragraphLines.length === 0) {
      return;
    }

    blocks.push({
      type: "paragraph",
      text: paragraphLines.join(" "),
    });
    paragraphLines = [];
  }

  function flushList() {
    if (listItems.length === 0) {
      return;
    }

    blocks.push({
      type: "list",
      items: [...listItems],
    });
    listItems = [];
  }

  function flushCode() {
    if (codeLines.length === 0) {
      return;
    }

    blocks.push({
      type: "code",
      text: codeLines.join("\n"),
    });
    codeLines = [];
  }

  for (const line of lines) {
    if (line.trimStart().startsWith("```")) {
      flushParagraph();
      flushList();
      if (inCodeBlock) {
        flushCode();
      }
      inCodeBlock = !inCodeBlock;
      continue;
    }

    if (inCodeBlock) {
      codeLines.push(line);
      continue;
    }

    const anchorMatch = /^<!--\s*anchor:([a-zA-Z0-9_-]+)\s*-->$/.exec(line.trim());
    if (anchorMatch) {
      flushParagraph();
      flushList();
      const anchorName = anchorMatch[1];
      const id = `anchor-${slugify(anchorName)}-${generatedCount++}`;
      blocks.push({
        type: "anchor",
        name: anchorName,
        id,
      });
      outline.push({
        id,
        label: `#${anchorName}`,
        level: 3,
        kind: "anchor",
      });
      continue;
    }

    const headingMatch = /^(#{1,6})\s+(.+)$/.exec(line);
    if (headingMatch) {
      flushParagraph();
      flushList();
      const level = headingMatch[1].length;
      const text = headingMatch[2].trim();
      const id = `section-${slugify(text)}-${generatedCount++}`;
      blocks.push({
        type: "heading",
        level,
        text,
        id,
      });
      outline.push({
        id,
        label: text,
        level,
        kind: "heading",
      });
      continue;
    }

    const listMatch = /^-\s+(.+)$/.exec(line);
    if (listMatch) {
      flushParagraph();
      listItems.push(listMatch[1].trim());
      continue;
    }

    if (!line.trim()) {
      flushParagraph();
      flushList();
      continue;
    }

    paragraphLines.push(line.trim());
  }

  flushParagraph();
  flushList();
  flushCode();

  if (blocks.length === 0 && normalized.trim()) {
    blocks.push({
      type: "code",
      text: normalized,
    });
  }

  const html = blocks.map((block) => {
    if (block.type === "heading") {
      return `<h${block.level} id="${block.id}" data-outline-id="${block.id}">${formatInlineText(block.text)}</h${block.level}>`;
    }

    if (block.type === "paragraph") {
      return `<p>${formatInlineText(block.text)}</p>`;
    }

    if (block.type === "list") {
      const items = block.items.map((item) => `<li>${formatInlineText(item)}</li>`).join("");
      return `<ul>${items}</ul>`;
    }

    if (block.type === "anchor") {
      return `<div class="document-anchor" id="${block.id}" data-outline-id="${block.id}">Anchor: ${escapeHtml(block.name)}</div>`;
    }

    if (block.type === "code") {
      return `<pre>${escapeHtml(block.text)}</pre>`;
    }

    return "";
  }).join("");

  return {
    html,
    outline,
  };
}

function renderOutline(outline) {
  elements.documentOutline.innerHTML = "";

  if (outline.length === 0) {
    elements.documentOutline.innerHTML = `<div class="document-outline-empty muted">No headings or anchors were found in this document yet.</div>`;
    return;
  }

  for (const item of outline) {
    const button = document.createElement("button");
    button.type = "button";
    button.className = `outline-button level-${Math.min(item.level, 6)} ${item.kind === "anchor" ? "outline-anchor" : ""}`;
    button.textContent = item.label;
    button.addEventListener("click", () => {
      const target = elements.documentView.querySelector(`[data-outline-id="${item.id}"]`);
      if (target) {
        target.scrollIntoView({
          block: "start",
          behavior: "smooth",
        });
      }
    });
    elements.documentOutline.appendChild(button);
  }
}

function renderDocument() {
  const model = buildDocumentModel();

  if (!model) {
    elements.documentKind.textContent = "Document";
    elements.documentTitle.textContent = "Select a journal or file";
    elements.documentSubtitle.textContent = "The main document will appear here so the middle pane stays focused on the content itself.";
    elements.documentOutlineMeta.textContent = "Select a journal or file to see headings and anchors here.";
    elements.documentView.innerHTML = `
      <div class="document-empty">
        <p class="muted">The center pane is ready for a journal entry or file preview.</p>
      </div>
    `;
    renderOutline([]);
    return;
  }

  const parsed = parseDocumentContent(model.content);
  const summaryCards = model.summaryCards.length > 0
    ? `<section class="document-summary">${model.summaryCards.map((card) => `
        <article class="document-meta-card">
          <strong>${escapeHtml(card.title)}</strong>
          <p>${escapeHtml(card.value)}</p>
        </article>
      `).join("")}</section>`
    : "";
  const tags = model.tags.length > 0
    ? `<div class="document-tags">${model.tags.map((tag) => `<span class="document-tag">${escapeHtml(tag)}</span>`).join("")}</div>`
    : "";
  const logs = model.logs.length > 0
    ? `<section class="journal-log-list">${model.logs.map((log) => `
        <article class="journal-log-item">
          <strong>${escapeHtml(log.name)}</strong>
          <p class="muted">Updated ${escapeHtml(formatTimestamp(log.updatedUtc))}</p>
          <code>${escapeHtml(log.relativePath)}</code>
        </article>
      `).join("")}</section>`
    : "";
  const logsSection = model.logs.length > 0
    ? `
      <section>
        <h3>Recent Logs</h3>
        ${logs}
      </section>
    `
    : "";

  elements.documentKind.textContent = model.kind;
  elements.documentTitle.textContent = model.title;
  elements.documentSubtitle.textContent = model.subtitle;
  elements.documentOutlineMeta.textContent = parsed.outline.length > 0
    ? "Use the outline to jump through headings and anchors in the current document."
    : "This document does not expose headings or anchors yet.";
  elements.documentView.innerHTML = `
    <div class="document-header-actions">
      ${model.actionLabel && model.actionPath ? `<button type="button" id="document-action-button" class="secondary-button">${escapeHtml(model.actionLabel)}</button>` : ""}
    </div>
    ${summaryCards}
    ${tags}
    <article class="document-rendered">
      ${parsed.html || `<p class="muted">No document content is available yet.</p>`}
    </article>
    ${logsSection}
  `;

  const actionButton = document.getElementById("document-action-button");
  if (actionButton && model.actionPath) {
    actionButton.addEventListener("click", () => {
      toggleAttachedFile(model.actionPath);
    });
  }

  renderOutline(parsed.outline);
}

function renderConversations() {
  elements.conversationList.innerHTML = "";

  if (state.conversations.length === 0) {
    elements.conversationList.innerHTML = `<p class="muted">No chats yet.</p>`;
    renderChatSwitcher();
    return;
  }

  for (const conversation of state.conversations) {
    const button = document.createElement("button");
    button.type = "button";
    button.className = "conversation-button";

    if (conversation.id === state.activeConversationId) {
      button.classList.add("active");
    }

    const title = document.createElement("strong");
    title.textContent = conversation.title;
    button.appendChild(title);

    const preview = document.createElement("span");
    preview.className = "conversation-preview";
    preview.textContent = conversation.preview
      ? conversation.preview
      : (conversation.journalTitle ? `Linked journal: ${conversation.journalTitle}` : "No messages yet.");
    button.appendChild(preview);

    if (conversation.journalTitle) {
      const journal = document.createElement("span");
      journal.className = "conversation-journal";
      journal.textContent = conversation.journalTitle;
      button.appendChild(journal);
    }

    button.addEventListener("click", () => loadConversation(conversation.id));
    elements.conversationList.appendChild(button);
  }

  renderChatSwitcher();
}

function renderChatSwitcher() {
  const activeConversation = state.activeConversation;
  const isCollapsed = state.chatListCollapsed;

  elements.chatSwitcherPanel.classList.toggle("collapsed", isCollapsed);
  elements.chatSwitcherToggle.textContent = isCollapsed ? "Show" : "Hide";
  elements.chatSwitcherToggle.setAttribute("aria-label", isCollapsed ? "Show chat list" : "Hide chat list");
  elements.chatSwitcherToggle.setAttribute("title", isCollapsed ? "Show chat list" : "Hide chat list");

  if (!elements.chatSwitcherSummary) {
    return;
  }

  if (!activeConversation) {
    elements.chatSwitcherSummary.textContent = state.conversations.length === 0
      ? "Create a chat to start the local thread."
      : "Recent chats stay visible here, with enough room to recognize what each one was about.";
    return;
  }

  elements.chatSwitcherSummary.textContent = isCollapsed
    ? `Active chat: ${activeConversation.title}`
    : "Recent chats stay visible here, with enough room to recognize what each one was about.";
}

function renderMessages(conversation = state.activeConversation) {
  const journalContext = buildJournalContextSummary(conversation);
  elements.chatJournalContext.textContent = journalContext.header;
  elements.composerContextStatus.textContent = journalContext.composer;
  elements.composerContextStatus.classList.toggle("hidden", !journalContext.composer);
  elements.messageList.innerHTML = "";

  if (!conversation) {
    elements.chatTitle.textContent = "Select or create a chat";
    return;
  }

  elements.chatTitle.textContent = conversation.title;

  if (!conversation.messages.length) {
    return;
  }

  for (const message of conversation.messages) {
    const article = document.createElement("article");
    article.className = `message ${message.role}`;
    article.innerHTML = `<div>${escapeHtml(message.content)}</div>`;

    if (message.fileContexts && message.fileContexts.length > 0) {
      const attachmentList = document.createElement("div");
      attachmentList.className = "message-file-contexts";

      for (const fileContext of message.fileContexts) {
        const attachment = document.createElement("div");
        attachment.className = "message-file-context";
        attachment.textContent = fileContext.relativePath;
        attachmentList.appendChild(attachment);
      }

      article.appendChild(attachmentList);
    }

    elements.messageList.appendChild(article);
  }

  elements.messageList.scrollTop = elements.messageList.scrollHeight;
}

function renderMemoryItems() {
  elements.memoryList.innerHTML = "";

  if (state.memoryItems.length === 0) {
    elements.memoryList.innerHTML = `<p class="muted">No saved memory yet.</p>`;
    return;
  }

  for (const item of state.memoryItems) {
    const article = document.createElement("article");
    article.className = "memory-item";
    article.innerHTML = `
      <h3>${escapeHtml(item.key)}</h3>
      <p>${escapeHtml(item.content)}</p>
      <p class="muted memory-meta">${item.pinned ? "Pinned into context" : "Saved only"}</p>
      <div class="memory-item-actions">
        <button type="button" class="secondary-button">Edit</button>
        <button type="button" class="secondary-button">Delete</button>
      </div>
    `;

    const [editButton, deleteButton] = article.querySelectorAll("button");
    editButton.addEventListener("click", () => beginMemoryEdit(item));
    deleteButton.addEventListener("click", () => deleteMemoryItem(item.id).catch(showError));
    elements.memoryList.appendChild(article);
  }
}

function renderJournalEntries() {
  elements.journalList.innerHTML = "";
  const effectiveEntryId = state.activeConversation?.journalEntryId ?? state.activeJournalEntryId;

  const generalButton = document.createElement("button");
  generalButton.type = "button";
  generalButton.className = "journal-button";
  if (!effectiveEntryId) {
    generalButton.classList.add("active");
  }

  generalButton.innerHTML = `<strong>General Chat</strong><span class="muted">No journal link</span>`;
  generalButton.addEventListener("click", () => {
    state.activeJournalEntryId = null;
    state.suppressRecentJournalFallback = true;
    renderJournalEntries();
    refreshJournalSelection().catch(showError);
  });
  elements.journalList.appendChild(generalButton);

  if (state.journalEntries.length === 0) {
    const empty = document.createElement("p");
    empty.className = "muted";
    empty.textContent = "No journal entries yet.";
    elements.journalList.appendChild(empty);
    return;
  }

  for (const entry of state.journalEntries) {
    const button = document.createElement("button");
    button.type = "button";
    button.className = "journal-button";

    if (entry.id === effectiveEntryId) {
      button.classList.add("active");
    }

    button.innerHTML = `
      <strong>${escapeHtml(entry.title)}</strong>
      <span class="muted">${escapeHtml(entry.summary || "No summary yet.")}</span>
      <span class="muted">Logs: ${entry.logCount}</span>
    `;

    button.addEventListener("click", () => {
      state.activeJournalEntryId = entry.id;
      state.suppressRecentJournalFallback = false;
      setActiveJournalDocument(entry.id);
      renderJournalEntries();
      refreshJournalSelection().catch(showError);
    });

    elements.journalList.appendChild(button);
  }
}

function renderSystemFlow() {
  elements.connectionList.innerHTML = "";

  if (!state.systemFlow) {
    elements.appSummary.textContent = "Local assistant shell";
    elements.contextSummary.textContent = "System flow is not available yet.";
    return;
  }

  elements.appSummary.textContent = state.systemFlow.summary;
  elements.contextSummary.textContent = state.systemFlow.context.summary;

  for (const connection of state.systemFlow.connections) {
    const article = document.createElement("article");
    article.className = "connection-item";
    article.innerHTML = `<strong>${escapeHtml(connection.name)} - ${escapeHtml(connection.status)}</strong><p class="muted">${escapeHtml(connection.detail)}</p>`;
    elements.connectionList.appendChild(article);
  }
}

function renderFileTree() {
  elements.fileTree.innerHTML = "";

  if (state.fileTree.length === 0) {
    elements.fileTree.innerHTML = `<p class="muted">No approved folders available yet.</p>`;
    return;
  }

  for (const node of state.fileTree) {
    elements.fileTree.appendChild(buildFileTreeNode(node));
  }
}

function buildFileTreeNode(node) {
  const wrapper = document.createElement("div");
  wrapper.className = "file-tree-node";

  const button = document.createElement("button");
  button.type = "button";
  button.className = `file-tree-button ${node.isDirectory ? "directory" : "file"}`;
  button.textContent = node.isDirectory ? `[Folder] ${node.name}` : node.name;

  const children = document.createElement("div");
  children.className = "file-tree-children";

  if (node.isDirectory) {
    button.addEventListener("click", () => {
      children.classList.toggle("hidden");
    });
  } else {
    button.draggable = true;
    button.addEventListener("click", () => {
      loadFilePreview(node.relativePath).catch(showError);
    });
    button.addEventListener("dragstart", (event) => {
      event.dataTransfer.setData(dragDataType, node.relativePath);
      event.dataTransfer.setData("text/plain", node.relativePath);
      event.dataTransfer.effectAllowed = "copy";
      button.classList.add("dragging-file");
    });
    button.addEventListener("dragend", () => {
      button.classList.remove("dragging-file");
      setComposerDropState(false);
    });
  }

  wrapper.appendChild(button);

  if (node.isDirectory) {
    for (const child of node.children) {
      children.appendChild(buildFileTreeNode(child));
    }
  }

  wrapper.appendChild(children);
  return wrapper;
}

function renderAttachedFiles() {
  elements.attachedFiles.innerHTML = "";
  elements.attachedFiles.classList.toggle("hidden", state.attachedFilePaths.length === 0);

  if (state.attachedFilePaths.length === 0) {
    return;
  }

  for (const filePath of state.attachedFilePaths) {
    const article = document.createElement("article");
    article.className = "attached-file-item";
    article.innerHTML = `
      <span>${escapeHtml(filePath)}</span>
      <button type="button" class="secondary-button">Remove</button>
    `;

    article.querySelector("button").addEventListener("click", () => {
      toggleAttachedFile(filePath);
    });

    elements.attachedFiles.appendChild(article);
  }
}

async function loadConversations() {
  state.conversations = await request("/api/chat/conversations");
  renderConversations();
}

async function loadMemoryItems() {
  state.memoryItems = await request("/api/memory/items");
  renderMemoryItems();
}

async function loadSystemFlow() {
  state.systemFlow = await request("/api/system/flow");
  renderSystemFlow();
}

async function loadJournalEntries() {
  state.journalEntries = await request("/api/journal/entries");

  if (state.activeJournalEntryId &&
      !state.journalEntries.some((entry) => entry.id === state.activeJournalEntryId)) {
    state.activeJournalEntryId = null;
  }

  renderJournalEntries();
  await refreshJournalSelection();
}

async function loadFileTree() {
  state.fileTree = await request("/api/files/tree");
  renderFileTree();
}

async function loadFilePreview(relativePath) {
  state.filePreview = await request(`/api/files/preview?path=${encodeURIComponent(relativePath)}`);
  setActiveFileDocument(state.filePreview.relativePath);
}

async function refreshJournalSelection() {
  state.journalSelection = resolveJournalSelection();

  if (!state.journalSelection) {
    state.activeJournalDetail = null;
    renderMessages();
    renderDocument();
    return null;
  }

  const selectedEntryId = state.journalSelection.entryId;
  if (!state.activeDocument || state.activeDocument.type === "journal") {
    state.activeDocument = {
      type: "journal",
      entryId: selectedEntryId,
    };
  }

  const cachedDetail = state.journalDetailCache[selectedEntryId];
  if (cachedDetail) {
    state.activeJournalDetail = cachedDetail;
    renderMessages();
    renderDocument();
    return state.journalSelection;
  }

  state.activeJournalDetail = null;
  renderMessages();
  renderDocument();

  const requestId = ++state.activeJournalDetailRequestId;
  const detail = await request(`/api/journal/entries/${selectedEntryId}`);
  if (requestId !== state.activeJournalDetailRequestId) {
    return state.journalSelection;
  }

  state.journalDetailCache[selectedEntryId] = detail;
  if (state.journalSelection?.entryId === selectedEntryId) {
    state.activeJournalDetail = detail;
    renderMessages();
    renderDocument();
  }

  return state.journalSelection;
}

async function loadConversation(conversationId) {
  const conversation = await request(`/api/chat/conversations/${conversationId}`);
  state.activeConversation = conversation;
  state.activeConversationId = conversation.id;
  state.chatListCollapsed = true;
  state.activeJournalEntryId = conversation.journalEntryId ?? null;
  state.suppressRecentJournalFallback = conversation.journalEntryId === null;

  if (conversation.journalEntryId) {
    state.activeDocument = {
      type: "journal",
      entryId: conversation.journalEntryId,
    };
  }

  renderConversations();
  renderJournalEntries();
  await refreshJournalSelection();
}

async function createConversation(journalEntryId = state.activeJournalEntryId) {
  const conversation = await request("/api/chat/conversations", {
    method: "POST",
    body: JSON.stringify({
      title: "New Chat",
      journalEntryId,
    }),
  });

  await loadConversations();
  await loadConversation(conversation.id);
}

async function sendMessage(content) {
  const selection = await refreshJournalSelection();
  const journalEntryId = selection?.entryId ?? null;

  if (!state.activeConversationId) {
    await createConversation(journalEntryId);
  }

  const conversation = await request(`/api/chat/conversations/${state.activeConversationId}/messages`, {
    method: "POST",
    body: JSON.stringify({
      content,
      filePaths: state.attachedFilePaths,
      journalEntryId,
    }),
  });

  state.activeConversation = conversation;
  state.activeConversationId = conversation.id;
  state.activeJournalEntryId = conversation.journalEntryId ?? state.activeJournalEntryId;

  if (journalEntryId) {
    delete state.journalDetailCache[journalEntryId];
  }

  await loadConversations();
  await loadSystemFlow();

  state.attachedFilePaths = [];
  renderAttachedFiles();
  await refreshJournalSelection();
}

async function saveJournalEntry() {
  const payload = {
    title: elements.journalTitleInput.value.trim(),
    summary: elements.journalSummaryInput.value.trim(),
    tags: [],
  };

  if (!payload.title) {
    return;
  }

  const entry = await request("/api/journal/entries", {
    method: "POST",
    body: JSON.stringify(payload),
  });

  elements.journalTitleInput.value = "";
  elements.journalSummaryInput.value = "";
  state.activeJournalEntryId = entry.id;
  state.activeDocument = {
    type: "journal",
    entryId: entry.id,
  };
  state.journalDetailCache = {};
  state.suppressRecentJournalFallback = false;
  await Promise.all([loadJournalEntries(), loadFileTree(), loadSystemFlow()]);
}

async function loadVoiceDraft() {
  return request("/api/voice/demo-transcript", {
    method: "POST",
  });
}

function resetMemoryForm() {
  state.editingMemoryId = null;
  elements.memoryKeyInput.value = "";
  elements.memoryContentInput.value = "";
  elements.memoryPinnedInput.checked = true;
  elements.memorySaveButton.textContent = "Save Memory";
  elements.memoryCancelButton.classList.add("hidden");
}

function beginMemoryEdit(item) {
  state.editingMemoryId = item.id;
  elements.memoryKeyInput.value = item.key;
  elements.memoryContentInput.value = item.content;
  elements.memoryPinnedInput.checked = item.pinned;
  elements.memorySaveButton.textContent = "Update Memory";
  elements.memoryCancelButton.classList.remove("hidden");
  elements.memoryKeyInput.focus();
}

async function saveMemoryItem() {
  const payload = {
    key: elements.memoryKeyInput.value.trim(),
    content: elements.memoryContentInput.value.trim(),
    pinned: elements.memoryPinnedInput.checked,
  };

  if (!payload.key || !payload.content) {
    return;
  }

  if (state.editingMemoryId) {
    await request(`/api/memory/items/${state.editingMemoryId}`, {
      method: "PUT",
      body: JSON.stringify(payload),
    });
  } else {
    await request("/api/memory/items", {
      method: "POST",
      body: JSON.stringify(payload),
    });
  }

  await Promise.all([loadMemoryItems(), loadSystemFlow()]);
  resetMemoryForm();
}

async function deleteMemoryItem(id) {
  await request(`/api/memory/items/${id}`, {
    method: "DELETE",
  });

  await Promise.all([loadMemoryItems(), loadSystemFlow()]);

  if (state.editingMemoryId === id) {
    resetMemoryForm();
  }
}

function attachFile(relativePath) {
  if (state.attachedFilePaths.includes(relativePath)) {
    return;
  }

  state.attachedFilePaths = [...state.attachedFilePaths, relativePath];
  renderAttachedFiles();
  renderDocument();
  refreshJournalSelection().catch(showError);
}

function removeAttachedFile(relativePath) {
  state.attachedFilePaths = state.attachedFilePaths.filter((path) => path !== relativePath);
  renderAttachedFiles();
  renderDocument();
  refreshJournalSelection().catch(showError);
}

function toggleAttachedFile(relativePath) {
  if (state.attachedFilePaths.includes(relativePath)) {
    removeAttachedFile(relativePath);
  } else {
    attachFile(relativePath);
  }
}

function setComposerDropState(isActive) {
  elements.chatForm.classList.toggle("drop-target-active", isActive);
  elements.composerDropHint.classList.toggle("hidden", !isActive);
  elements.composerDropHint.classList.toggle("composer-drop-active", isActive);
}

function hasDraggedFile(event) {
  const types = Array.from(event.dataTransfer?.types ?? []);
  return types.includes(dragDataType) || types.includes("text/plain");
}

function readDraggedFilePath(event) {
  const customPath = event.dataTransfer?.getData(dragDataType)?.trim();
  if (customPath) {
    return customPath;
  }

  return event.dataTransfer?.getData("text/plain")?.trim() ?? "";
}

function showError(error) {
  console.error(error);
  alert("Something went wrong while talking to the local app.");
}

elements.newChatButton.addEventListener("click", () => {
  createConversation().catch(showError);
});

elements.chatSwitcherToggle.addEventListener("click", () => {
  state.chatListCollapsed = !state.chatListCollapsed;
  renderChatSwitcher();
});

elements.voiceDraftButton.addEventListener("click", async () => {
  if (state.dictationActive) {
    await stopActiveDictation();
    return;
  }

  let localSessionError = null;

  try {
    if (await startLocalSessionDictation()) {
      return;
    }
  } catch (error) {
    localSessionError = error;
    console.error(error);
  }

  try {
    if (startBrowserDictation()) {
      if (localSessionError) {
        alert(`Local dictation session could not start: ${describeDictationError(localSessionError)}. Using the browser fallback for now.`);
      }
      return;
    }

    const transcript = await loadVoiceDraft();
    const reason = localSessionError
      ? `Dictation fallback used because the local dictation session could not start: ${describeDictationError(localSessionError)}.`
      : "Dictation fallback used because live microphone capture is not available in this browser yet.";
    elements.messageInput.value = `${reason} ${transcript.text}`.trim();
    elements.messageInput.focus();
  } catch (error) {
    showError(error);
  }
});

elements.memoryCancelButton.addEventListener("click", () => {
  resetMemoryForm();
});

elements.memoryForm.addEventListener("submit", async (event) => {
  event.preventDefault();

  try {
    await saveMemoryItem();
  } catch (error) {
    showError(error);
  }
});

elements.journalForm.addEventListener("submit", async (event) => {
  event.preventDefault();

  try {
    await saveJournalEntry();
  } catch (error) {
    showError(error);
  }
});

elements.chatForm.addEventListener("submit", async (event) => {
  event.preventDefault();
  const content = elements.messageInput.value.trim();
  if (!content) {
    return;
  }

  elements.messageInput.value = "";

  try {
    await sendMessage(content);
  } catch (error) {
    showError(error);
  }
});

elements.messageInput.addEventListener("keydown", (event) => {
  if (event.key !== "Enter" || event.shiftKey || event.isComposing) {
    return;
  }

  event.preventDefault();
  elements.chatForm.requestSubmit();
});

elements.chatForm.addEventListener("dragenter", (event) => {
  if (!hasDraggedFile(event)) {
    return;
  }

  event.preventDefault();
  setComposerDropState(true);
});

elements.chatForm.addEventListener("dragover", (event) => {
  if (!hasDraggedFile(event)) {
    return;
  }

  event.preventDefault();
  event.dataTransfer.dropEffect = "copy";
  setComposerDropState(true);
});

elements.chatForm.addEventListener("dragleave", (event) => {
  if (!elements.chatForm.contains(event.relatedTarget)) {
    setComposerDropState(false);
  }
});

elements.chatForm.addEventListener("drop", (event) => {
  if (!hasDraggedFile(event)) {
    setComposerDropState(false);
    return;
  }

  const relativePath = readDraggedFilePath(event);
  if (!relativePath) {
    setComposerDropState(false);
    return;
  }

  event.preventDefault();
  attachFile(relativePath);
  setComposerDropState(false);
});

Promise.all([loadSystemFlow(), loadConversations(), loadMemoryItems(), loadJournalEntries(), loadFileTree()])
  .then(async () => {
    resetMemoryForm();
    updateVoiceDraftButton();
    renderAttachedFiles();
    setComposerDropState(false);
    renderDocument();
    renderChatSwitcher();
    await refreshJournalSelection();
  })
  .catch(showError);
